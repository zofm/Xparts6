using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Smartstore.Core.Checkout.Orders;
using Smartstore.Core.Common;
using Smartstore.Core.Data;
using Smartstore.Core.Identity;
using Smartstore.FatturazioneElettronica.Domain;
using Smartstore.FatturazioneElettronica.Models;
using Smartstore.FatturazioneElettronica.Settings;
using Smartstore.FatturazioneElettronica.XML;
using Smartstore.FatturazioneElettronica.XML.Defaults;
using Unidecode.NET;

namespace Smartstore.FatturazioneElettronica.Services
{
    public partial class FatturazioneService : IFatturazioneService
    {
        private readonly SmartDbContext _db;
        private readonly IOrderService _orderService;
        private readonly IFatturazioneCountryService _fatturazioneCountryService;
        private readonly FatturazioneElettronicaSettings _fatturazioneSettings;

        private readonly Regex _regex = new Regex(@".+\s\((?<num>N[\d\.]+)\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public ILogger Logger { get; set; } = NullLogger.Instance;

        public FatturazioneService(
            SmartDbContext db,
            IOrderService orderService,
            IFatturazioneCountryService fatturazioneCountryService,
            FatturazioneElettronicaSettings fatturazioneSettings)
        {
            _db = db;
            _orderService = orderService;
            _fatturazioneCountryService = fatturazioneCountryService;
            _fatturazioneSettings = fatturazioneSettings;
        }

        #region Private helpers

        private string GetCodiceDestinatario(string country, string sdiCode, bool isCessionarioPrivato)
        {
            sdiCode ??= "0000000";

            if (!isCessionarioPrivato)
            {
                if (country == "IT")
                {
                    var codiceDestinatario = sdiCode;
                    try { var m = new MailAddress(sdiCode); codiceDestinatario = "0000000"; }
                    catch (FormatException) { }
                    return codiceDestinatario.ToUpper().Trim();
                }
                else return "XXXXXXX";
            }
            else return country == "IT" ? "0000000" : "XXXXXXX";
        }

        private string GetPECDestinatario(string country, string sdiCode, bool isCessionarioPrivato)
        {
            string pecDestinatario = null;
            if (!isCessionarioPrivato && country == "IT")
            {
                try { var m = new MailAddress(sdiCode); pecDestinatario = sdiCode; }
                catch { }
            }
            return pecDestinatario;
        }

        private Cessionario GetCessionario(Address billingAddress, string vatCode, string taxCode, bool isCessionarioPrivato, string vatCountryCode)
        {
            var cessionario = new Cessionario
            {
                Indirizzo = billingAddress.Address1,
                Comune = billingAddress.City,
                Provincia = billingAddress.StateProvince?.Abbreviation,
                Paese = billingAddress.Country.TwoLetterIsoCode,   // physical address: keep as-is
                Cap = billingAddress.ZipPostalCode
            };

            if (isCessionarioPrivato && vatCountryCode == "IT")
            {
                cessionario.TipoSoggetto = TipoSoggetto.Privato;
                cessionario.Nome = billingAddress.FirstName;
                cessionario.Cognome = billingAddress.LastName;
                cessionario.CodiceFiscale = GetCodiceFiscaleCessionario(vatCountryCode, taxCode);
            }
            else if (isCessionarioPrivato && vatCountryCode != "IT")
            {
                cessionario.TipoSoggetto = TipoSoggetto.Azienda;
                cessionario.Nome = billingAddress.FirstName;
                cessionario.Cognome = billingAddress.LastName;
                cessionario.IdFiscaleIva = new IdFiscaleIva
                {
                    CodicePaese = vatCountryCode,
                    PartitaIva = GetPartitaIvaCessionario(vatCode, taxCode, vatCountryCode)
                };
            }
            else
            {
                cessionario.TipoSoggetto = TipoSoggetto.Azienda;
                cessionario.Denominazione = billingAddress.Company;
                cessionario.PartitaIva = vatCode;
                cessionario.IdFiscaleIva = new IdFiscaleIva
                {
                    CodicePaese = vatCountryCode,
                    PartitaIva = GetPartitaIvaCessionario(vatCode, taxCode, vatCountryCode)
                };
            }

            return cessionario;
        }

        private static string GetCodiceFiscaleCessionario(string vatCountryCode, string taxCode)
        {
            if (vatCountryCode.Equals("IT", StringComparison.OrdinalIgnoreCase))
                return taxCode?.Trim().ToUpper();
            return null;
        }

        private string GetPartitaIvaCessionario(string vatCode, string taxCode, string vatCountryCode)
        {
            var countryCodesInsideUE = _fatturazioneCountryService.GetEuropeanCountryTwoLetterIsoCodes();
            if (countryCodesInsideUE.Contains(vatCountryCode))
            {
                if (!string.IsNullOrEmpty(taxCode))
                    return taxCode.Trim().ToUpper();
                return "OO99999999999";
            }
            return "OO99999999999";
        }

        /// <summary>
        /// Extracts the 2-letter country code from the first two characters of a VAT number.
        /// Falls back to the billing address country when the VAT is absent or does not start with letters.
        /// </summary>
        private static string GetCountryCodeFromVat(string vatCode, string billingAddressCountry)
        {
            if (!string.IsNullOrWhiteSpace(vatCode)
                && vatCode.Length >= 2
                && char.IsAsciiLetter(vatCode[0])
                && char.IsAsciiLetter(vatCode[1]))
            {
                return vatCode[..2].ToUpper();
            }
            return billingAddressCountry?.ToUpper();
        }

        private ModalitaPagamento GetModalitaPagamento(string paymentSystemName)
        {
            return paymentSystemName switch
            {
                "Payments.BankTransfer" or "Payments.Prepayment" => ModalitaPagamento.Bonifico,
                "Payments.CreditCard" or "Smartstore.PagOnline" or "SmartStore.Klarna" => ModalitaPagamento.CartaDiPagamento,
                "Payments.CashOnDelivery" => ModalitaPagamento.Contanti,
                _ => ModalitaPagamento.CartaDiPagamento
            };
        }

        #endregion

        #region XML generation

        public FileInfo CreateInvoiceXml(int orderId)
        {
            var order = _db.Orders
                .Include(x => x.Customer)
                .Include(x => x.BillingAddress.Country)
                .Include(x => x.BillingAddress.StateProvince)
                .Include(x => x.OrderItems).ThenInclude(x => x.Product)
                .FirstOrDefault(x => x.Id == orderId);

            var invoice = _db.FEInvoices().FirstOrDefault(x => x.OrderId == orderId);

            Guard.NotNull(order, nameof(order));
            Guard.NotNull(invoice, nameof(invoice));

            var customer = order.Customer;
            var customerVatCode = customer.GenericAttributes.VatNumber?.Trim();
            var customerSdiCode = customer.GenericAttributes.Get<string>("SdiCode")?.Trim();
            var isCessionarioPrivato = string.IsNullOrWhiteSpace(order.Customer.Company);
            // Derive fiscal country from the VAT prefix; fall back to billing address country for private customers
            var vatCountryCode = GetCountryCodeFromVat(customerVatCode, order.BillingAddress.Country.TwoLetterIsoCode);
            var countryCodesInsideUE = _fatturazioneCountryService.GetEuropeanCountryTwoLetterIsoCodes();

            if (!isCessionarioPrivato && string.IsNullOrWhiteSpace(customerVatCode))
            {
                Logger.Warn($"Errore fatturazione (verso azienda): per l'ordine n. {order.Id} (fattura n. X{invoice.Number}/{invoice.Year}) non è presente la partita IVA.");
                return null;
            }
            if (!isCessionarioPrivato && countryCodesInsideUE.Contains(vatCountryCode) && string.IsNullOrWhiteSpace(customerSdiCode))
            {
                Logger.Warn($"Errore fatturazione (verso azienda): per l'ordine n. {order.Id} (fattura n. X{invoice.Number}/{invoice.Year}) non è presente il codice SDI.");
                return null;
            }
            if (isCessionarioPrivato && vatCountryCode == "IT" && string.IsNullOrWhiteSpace(customerVatCode))
            {
                Logger.Warn($"Errore fatturazione (verso privato): per l'ordine n. {order.Id} (fattura n. X{invoice.Number}/{invoice.Year}) non è presente il codice fiscale.");
                return null;
            }

            var invoiceNumber = invoice.Number;
            var invoiceDate = invoice.CreatedOnUtc.Value;

            EsenzioneIva? invoiceExemptionType = null;
            if (invoice.ExemptionId.HasValue)
                invoiceExemptionType = (EsenzioneIva)invoice.ExemptionId;

            decimal discountTotal = 0M;

            var fatturaModel = new FatturaModel(DestinatarioFattura.Privato)
            {
                ProgressivoInvio = $"XP{order.Id}",
                IdCodiceTrasmissione = _fatturazioneSettings.ArubaTaxCode,
                IdPaeseTrasmissione = "IT",
                CodiceDestinatario = GetCodiceDestinatario(vatCountryCode, customerSdiCode, isCessionarioPrivato),
                PECDestinatario = GetPECDestinatario(vatCountryCode, customerSdiCode, isCessionarioPrivato),
                Intestazione = new Intestazione
                {
                    TipoDocumento = TipoDocumento.Fattura,
                    NumeroDocumento = string.Format(_fatturazioneSettings.InvoiceNumberPattern, invoiceNumber, invoiceDate.Year),
                    DataEmissione = invoiceDate,
                    Divisa = Divisa.EUR,
                    EsigibilitaIva = EsigibilitaIva.Immediata,
                    Causale = !string.IsNullOrEmpty(invoice.Causal) ? invoice.Causal.Replace("\n", " ").Replace("\r", "") : "Vendita prodotti online"
                },
                Cedente = new Cedente
                {
                    TipoSoggetto = TipoSoggetto.Azienda,
                    RegimeFiscale = RegimeFiscale.Ordinario,
                    CodiceEORI = _fatturazioneSettings.EORI,
                    Denominazione = _fatturazioneSettings.CompanyName,
                    Indirizzo = _fatturazioneSettings.Address,
                    NumeroCivico = _fatturazioneSettings.AddressNumber,
                    Comune = _fatturazioneSettings.City,
                    Provincia = _fatturazioneSettings.Province,
                    Paese = _fatturazioneSettings.Country,
                    Cap = _fatturazioneSettings.ZipCode,
                    CodiceFiscale = _fatturazioneSettings.TaxCode,
                    PartitaIva = _fatturazioneSettings.VatCode,
                    IdFiscaleIva = new IdFiscaleIva
                    {
                        PartitaIva = _fatturazioneSettings.VatCode,
                        CodicePaese = _fatturazioneSettings.Country
                    }
                },
                Cessionario = GetCessionario(order.BillingAddress, customerVatCode, customerVatCode, isCessionarioPrivato, vatCountryCode)
            };

            var vatPercentage = order.TaxRatesDictionary.OrderByDescending(x => x.Key).FirstOrDefault().Key;

            order.OrderItems.ToList().ForEach(x =>
            {
                EsenzioneIva? productExemption = null;
                // TaxCategory navigation not loaded; exemption is determined at invoice level only

                fatturaModel.Dettagli.Add(new Dettaglio
                {
                    Descrizione = x.Product.Name,
                    Quantita = x.Quantity,
                    PrezzoUnitario = x.UnitPriceExclTax + x.DiscountAmountExclTax,
                    AliquotaIva = productExemption.HasValue ? 0M : (invoice.ExemptionId.HasValue ? 0M : vatPercentage),
                    EsenzioneIva = productExemption ?? invoiceExemptionType,
                });

                if (x.DiscountAmountExclTax > 0)
                {
                    fatturaModel.Dettagli.Add(new Dettaglio
                    {
                        TipoPrestazione = TipoPrestazione.Sconto,
                        Descrizione = "Sconto su articoli precedenti",
                        Quantita = 1,
                        PrezzoUnitario = -(x.DiscountAmountExclTax * x.Quantity),
                        AliquotaIva = invoice.ExemptionId.HasValue ? 0M : vatPercentage,
                        EsenzioneIva = invoiceExemptionType,
                    });
                }
            });

            if (order.PaymentMethodAdditionalFeeExclTax > 0)
            {
                fatturaModel.Dettagli.Add(new Dettaglio
                {
                    Descrizione = "Spese di pagamento",
                    Quantita = 1,
                    PrezzoUnitario = order.PaymentMethodAdditionalFeeExclTax,
                    AliquotaIva = invoice.ExemptionId.HasValue ? 0M : vatPercentage,
                    EsenzioneIva = invoiceExemptionType,
                });
            }

            if (order.OrderShippingExclTax > 0)
            {
                fatturaModel.Dettagli.Add(new Dettaglio
                {
                    Descrizione = "Spese di spedizione",
                    Quantita = 1,
                    PrezzoUnitario = order.OrderShippingExclTax,
                    AliquotaIva = invoice.ExemptionId.HasValue ? 0M : vatPercentage,
                    EsenzioneIva = invoiceExemptionType,
                });
            }

            if (order.OrderSubTotalDiscountExclTax > 0)
            {
                fatturaModel.Dettagli.Add(new Dettaglio
                {
                    Descrizione = "Sconto fisso sul totale",
                    Quantita = 1,
                    PrezzoUnitario = -(order.OrderSubTotalDiscountExclTax),
                    TipoPrestazione = TipoPrestazione.Sconto,
                    AliquotaIva = invoice.ExemptionId.HasValue ? 0M : vatPercentage,
                    EsenzioneIva = invoiceExemptionType
                });
                discountTotal += invoice.ExemptionId.HasValue ? order.OrderSubTotalDiscountExclTax : order.OrderSubTotalDiscountInclTax;
            }
            else if (order.OrderDiscount > 0)
            {
                fatturaModel.Dettagli.Add(new Dettaglio
                {
                    Descrizione = "Sconto fisso sul totale",
                    Quantita = 1,
                    PrezzoUnitario = -(order.OrderDiscount),
                    TipoPrestazione = TipoPrestazione.Sconto,
                    AliquotaIva = invoice.ExemptionId.HasValue ? 0M : vatPercentage,
                    EsenzioneIva = invoiceExemptionType
                });
                discountTotal += invoice.ExemptionId.HasValue ? order.OrderDiscount : order.OrderDiscount + Math.Round((order.OrderDiscount * vatPercentage) / 100, 2);
            }

            fatturaModel.Pagamento = new Pagamento
            {
                CondizioniPagamento = CondizioniPagamento.Completo,
                Dettagli = new List<DettaglioPagamento>
                {
                    new DettaglioPagamento
                    {
                        ModalitaPagamento = GetModalitaPagamento(order.PaymentMethodSystemName),
                        DataScadenza = invoiceDate.AddDays(7),
                        Importo = Math.Round(order.OrderSubtotalInclTax + order.OrderShippingInclTax - discountTotal, 2),
                        Imposta = order.OrderTax,
                    }
                }
            };

            var xml = CreateInvoiceXmlFile(invoice.Id, fatturaModel, orderId, invoice.Number, invoice.Year);

            invoice.HasXmlFile = true;
            _db.SaveChanges();

            return xml;
        }

        private FileInfo CreateInvoiceXmlFile(int invoiceId, FatturaModel fattura, int orderId, int invoiceNumber, int invoiceYear)
        {
            if (fattura.DestinatarioFattura != DestinatarioFattura.Privato && string.IsNullOrWhiteSpace(fattura.CodiceDestinatario))
                throw new Exception("Se il destinatario non è un privato, il \"CodiceDestinatario\" è obbligatorio!");

            var fatt = FatturaBase.CreateInstance(fattura.DestinatarioFattura == DestinatarioFattura.Privato ? Instance.Privati : Instance.PubblicaAmministrazione);

            fatt.FatturaElettronicaHeader.DatiTrasmissione.CodiceDestinatario = fattura.CodiceDestinatario;
            fatt.FatturaElettronicaHeader.DatiTrasmissione.PECDestinatario = fattura.PECDestinatario;
            fatt.FatturaElettronicaHeader.DatiTrasmissione.IdTrasmittente = new XML.FatturaElettronicaHeader.DatiTrasmissione.IdTrasmittente
            {
                IdCodice = fattura.IdCodiceTrasmissione,
                IdPaese = fattura.IdPaeseTrasmissione,
            };
            fatt.FatturaElettronicaHeader.DatiTrasmissione.ProgressivoInvio = fattura.ProgressivoInvio;

            // Cedente
            if (fattura.Cedente.TipoSoggetto == TipoSoggetto.Azienda || fattura.Cedente.TipoSoggetto == TipoSoggetto.PubblicaAmministrazione)
            {
                fatt.FatturaElettronicaHeader.CedentePrestatore.DatiAnagrafici.Anagrafica.Denominazione = fattura.Cedente.Denominazione;
                fatt.FatturaElettronicaHeader.CedentePrestatore.DatiAnagrafici.Anagrafica.CodEORI = fattura.Cedente.CodiceEORI;
                fatt.FatturaElettronicaHeader.CedentePrestatore.DatiAnagrafici.RegimeFiscale = fattura.Cedente.RegimeFiscale.ToDescription();
                fatt.FatturaElettronicaHeader.CedentePrestatore.DatiAnagrafici.IdFiscaleIVA = new XML.Common.IdFiscaleIVA
                {
                    IdCodice = fattura.Cedente.IdFiscaleIva.PartitaIva,
                    IdPaese = fattura.Cedente.IdFiscaleIva.CodicePaese
                };
            }
            else
            {
                fatt.FatturaElettronicaHeader.CedentePrestatore.DatiAnagrafici.Anagrafica.Nome = Utilities.Normalizza(fattura.Cedente.Nome);
                fatt.FatturaElettronicaHeader.CedentePrestatore.DatiAnagrafici.Anagrafica.Cognome = Utilities.Normalizza(fattura.Cedente.Cognome);
                fatt.FatturaElettronicaHeader.CedentePrestatore.DatiAnagrafici.CodiceFiscale = fattura.Cedente.CodiceFiscale;
            }

            fatt.FatturaElettronicaHeader.CedentePrestatore.Sede = new XML.FatturaElettronicaHeader.CedentePrestatore.SedeCedentePrestatore
            {
                Indirizzo = fattura.Cedente.Indirizzo,
                CAP = fattura.Cedente.Cap,
                Comune = fattura.Cedente.Comune,
                Nazione = fattura.Cedente.Paese,
                Provincia = fattura.Cedente.Provincia,
                NumeroCivico = fattura.Cedente.NumeroCivico
            };

            // Cessionario
            var estero = fattura.Cessionario.Paese.ToLower() != "it";

            if (fattura.Cessionario.TipoSoggetto == TipoSoggetto.Azienda || fattura.Cessionario.TipoSoggetto == TipoSoggetto.PubblicaAmministrazione)
            {
                fatt.FatturaElettronicaHeader.CessionarioCommittente.DatiAnagrafici.Anagrafica.Denominazione = estero ? fattura.Cessionario.Denominazione.Unidecode() : fattura.Cessionario.Denominazione;
                fatt.FatturaElettronicaHeader.CessionarioCommittente.DatiAnagrafici.Anagrafica.CodEORI = fattura.Cessionario.CodiceEORI;
                fatt.FatturaElettronicaHeader.CessionarioCommittente.DatiAnagrafici.IdFiscaleIVA = new XML.Common.IdFiscaleIVA
                {
                    IdCodice = fattura.Cessionario.IdFiscaleIva.PartitaIva,
                    IdPaese = fattura.Cessionario.IdFiscaleIva.CodicePaese
                };

                if (estero && fattura.Cessionario.Nome != null && fattura.Cessionario.Cognome != null)
                {
                    fatt.FatturaElettronicaHeader.CessionarioCommittente.DatiAnagrafici.Anagrafica.Nome = Utilities.Normalizza(fattura.Cessionario.Nome);
                    fatt.FatturaElettronicaHeader.CessionarioCommittente.DatiAnagrafici.Anagrafica.Cognome = Utilities.Normalizza(fattura.Cessionario.Cognome);
                }
            }
            else
            {
                fatt.FatturaElettronicaHeader.CessionarioCommittente.DatiAnagrafici.Anagrafica.Nome = estero ? fattura.Cessionario.Nome.Unidecode() : Utilities.Normalizza(fattura.Cessionario.Nome);
                fatt.FatturaElettronicaHeader.CessionarioCommittente.DatiAnagrafici.Anagrafica.Cognome = estero ? fattura.Cessionario.Cognome.Unidecode() : Utilities.Normalizza(fattura.Cessionario.Cognome);
                if (estero)
                {
                    fatt.FatturaElettronicaHeader.CessionarioCommittente.DatiAnagrafici.IdFiscaleIVA = new XML.Common.IdFiscaleIVA
                    {
                        IdCodice = string.Format("{0}{1}{2}",
                            fattura.Cessionario.Paese,
                            Utilities.TogliCaratteri(fattura.Cessionario.Nome.Unidecode().ToLower()),
                            Utilities.TogliCaratteri(fattura.Cessionario.Cognome.Unidecode().ToLower())),
                        IdPaese = fattura.Cessionario.Paese
                    };
                }
                else
                {
                    fatt.FatturaElettronicaHeader.CessionarioCommittente.DatiAnagrafici.CodiceFiscale = fattura.Cessionario.CodiceFiscale;
                }
            }

            fatt.FatturaElettronicaHeader.CessionarioCommittente.Sede = new XML.FatturaElettronicaHeader.CessionarioCommittente.SedeCessionarioCommittente
            {
                Indirizzo = estero ? fattura.Cessionario.Indirizzo.Unidecode() : Utilities.Normalizza(fattura.Cessionario.Indirizzo),
                CAP = estero ? "00000" : fattura.Cessionario.Cap,
                Comune = estero ? fattura.Cessionario.Comune.Unidecode() : Utilities.Normalizza(fattura.Cessionario.Comune),
                Nazione = fattura.Cessionario.Paese,
                Provincia = estero ? null : fattura.Cessionario.Provincia,
                NumeroCivico = fattura.Cessionario.NumeroCivico
            };

            fatt.FatturaElettronicaBody.Add(new XML.FatturaElettronicaBody.FatturaElettronicaBody
            {
                DatiGenerali = new XML.FatturaElettronicaBody.DatiGenerali.DatiGenerali
                {
                    DatiGeneraliDocumento = new XML.FatturaElettronicaBody.DatiGenerali.DatiGeneraliDocumento
                    {
                        TipoDocumento = fattura.Intestazione.TipoDocumento.ToDescription(),
                        Data = fattura.Intestazione.DataEmissione,
                        Divisa = fattura.Intestazione.Divisa.ToString(),
                        Numero = fattura.Intestazione.NumeroDocumento,
                        Causale = new List<string> { fattura.Intestazione.Causale },
                    }
                },
                DatiBeniServizi = new XML.FatturaElettronicaBody.DatiBeniServizi.DatiBeniServizi
                {
                    DettaglioLinee = new List<XML.FatturaElettronicaBody.DatiBeniServizi.DettaglioLinee>(),
                    DatiRiepilogo = new List<XML.FatturaElettronicaBody.DatiBeniServizi.DatiRiepilogo>()
                }
            });

            int i = 1;
            fattura.Dettagli.ForEach(d =>
            {
                var dettaglioLinea = new XML.FatturaElettronicaBody.DatiBeniServizi.DettaglioLinee
                {
                    NumeroLinea = i,
                    Descrizione = d.Descrizione,
                    Quantita = d.Quantita,
                    PrezzoUnitario = Math.Round(d.PrezzoUnitario, 2, MidpointRounding.AwayFromZero),
                    PrezzoTotale = Math.Round(d.PrezzoTotale, 2, MidpointRounding.AwayFromZero),
                    AliquotaIVA = d.AliquotaIva
                };

                if (d.TipoPrestazione == TipoPrestazione.Sconto)
                    dettaglioLinea.TipoCessionePrestazione = "SC";

                if (d.EsenzioneIva.HasValue)
                {
                    dettaglioLinea.AliquotaIVA = 0M;
                    dettaglioLinea.Natura = d.EsenzioneIva.Value.ToDescription();
                }

                if (d.Sconti != null)
                {
                    dettaglioLinea.ScontoMaggiorazione = d.Sconti.Select(x => new XML.Common.ScontoMaggiorazione
                    {
                        Tipo = "SC",
                        Percentuale = x.TipoSconto == TipoSconto.Percentuale ? x.Valore : (decimal?)null,
                        Importo = x.TipoSconto == TipoSconto.Importo ? x.Valore : (decimal?)null
                    }).ToList();
                }

                fatt.FatturaElettronicaBody[0].DatiBeniServizi.DettaglioLinee.Add(dettaglioLinea);
                i++;
            });

            decimal totalePagamento = 0M;

            fattura.Dettagli
                .GroupBy(x => new { x.AliquotaIva, x.EsenzioneIva })
                .ToList()
                .ForEach(x =>
                {
                    if (!x.Key.EsenzioneIva.HasValue)
                    {
                        fatt.FatturaElettronicaBody[0].DatiBeniServizi.DatiRiepilogo.Add(new XML.FatturaElettronicaBody.DatiBeniServizi.DatiRiepilogo
                        {
                            AliquotaIVA = x.Key.AliquotaIva,
                            ImponibileImporto = x.Sum(y => y.PrezzoTotale),
                            Imposta = x.Sum(y => y.ImpostaTotale),
                            EsigibilitaIVA = fattura.Intestazione.EsigibilitaIva.ToDescription()
                        });
                        totalePagamento += x.Sum(y => y.ImpostaTotale);
                    }
                    else
                    {
                        fatt.FatturaElettronicaBody[0].DatiBeniServizi.DatiRiepilogo.Add(new XML.FatturaElettronicaBody.DatiBeniServizi.DatiRiepilogo
                        {
                            AliquotaIVA = 0,
                            ImponibileImporto = x.Sum(y => y.PrezzoTotale),
                            Imposta = 0,
                            Natura = x.Key.EsenzioneIva.Value.ToDescription(),
                            RiferimentoNormativo = Utilities.RitornaDescrizioneEsenzione(x.Key.EsenzioneIva.Value)
                        });
                    }
                    totalePagamento += x.Sum(y => y.PrezzoTotale);
                });

            if (fattura.Pagamento != null)
            {
                fatt.FatturaElettronicaBody[0].DatiPagamento = new List<XML.FatturaElettronicaBody.DatiPagamento.DatiPagamento>
                {
                    new XML.FatturaElettronicaBody.DatiPagamento.DatiPagamento
                    {
                        CondizioniPagamento = fattura.Pagamento.CondizioniPagamento.ToDescription()
                    }
                };

                fattura.Pagamento.Dettagli.ForEach(p =>
                {
                    fatt.FatturaElettronicaBody[0].DatiPagamento[0].DettaglioPagamento.Add(new XML.FatturaElettronicaBody.DatiPagamento.DettaglioPagamento
                    {
                        ImportoPagamento = totalePagamento,
                        ModalitaPagamento = p.ModalitaPagamento.ToDescription(),
                        DataScadenzaPagamento = p.DataScadenza,
                    });
                });
            }

            var xmlFile = new FileInfo(Path.Combine(_fatturazioneSettings.AppDataFolder, _fatturazioneSettings.WaitingFolderName, $"O{orderId}-X{invoiceNumber}.{invoiceId}.xml"));
            if (!xmlFile.Directory.Exists)
                xmlFile.Directory.Create();

            using (var w = XmlWriter.Create(xmlFile.FullName, new XmlWriterSettings { Indent = true }))
                fatt.WriteXml(w);

            return xmlFile;
        }

        #endregion

        #region Invoice CRUD

        public Invoice GetInvoiceByOrderId(int orderId)
            => _db.FEInvoices().AsNoTracking().FirstOrDefault(x => x.OrderId == orderId);

        public int GetLastInvoiceNumber(int year)
            => _db.FEInvoices()
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Number)
                .Where(x => x.Year == year)
                .Select(x => x.Number)
                .FirstOrDefault();

        public void CreateInvoice(int orderId, int? exemptionId, string causal)
        {
            var utcNow = DateTime.UtcNow;
            var order = _db.Orders.Find(orderId);
            if (order == null)
                throw new Exception("Order not found!");

            var invoiceNumber = GetLastInvoiceNumber(utcNow.Year) + 1;

            _db.FEInvoices().Add(new Invoice
            {
                CreatedOnUtc = utcNow,
                OrderId = orderId,
                Number = invoiceNumber,
                Year = utcNow.Year,
                ExemptionId = exemptionId,
                Causal = causal
            });
            _db.SaveChanges();
        }

        public void RecreateInvoice(int orderId, int invoiceId, int? exemptionId, string causal)
        {
            var order = _db.Orders.Find(orderId);
            if (order == null)
                throw new Exception("Order not found!");

            var invoice = _db.FEInvoices().FirstOrDefault(x => x.Id == invoiceId);
            if (invoice == null)
                throw new Exception("Invoice not found!");

            invoice.Causal = causal;
            invoice.ExemptionId = exemptionId;
            invoice.HasXmlFile = false;
            _db.SaveChanges();

            _db.FEInvoiceHistories().Add(new InvoiceHistory
            {
                Status = InvoiceStatus.SentToSdi,
                CreatedOnUtc = DateTime.UtcNow,
                InvoiceId = invoiceId
            });
            _db.SaveChanges();
        }

        public void UpdateInvoice(Invoice record)
        {
            Guard.NotNull(record);
            record.UpdatedOnUtc = DateTime.UtcNow;
            _db.SaveChanges();
        }

        public void DeleteInvoice(Invoice record)
        {
            Guard.NotNull(record);
            _db.FEInvoices().Remove(record);
            _db.SaveChanges();
            DeleteInvoiceFile(record.Id);
        }

        public void DeleteInvoiceByOrderId(int orderId)
        {
            var record = _db.FEInvoices().FirstOrDefault(x => x.OrderId == orderId);
            if (record != null)
            {
                _db.FEInvoices().Remove(record);
                _db.SaveChanges();
                DeleteInvoiceFile(record.Id);
            }
        }

        private void DeleteInvoiceFile(int invoiceId)
        {
            var file = new FileInfo(Path.Combine(_fatturazioneSettings.AppDataFolder, _fatturazioneSettings.WaitingFolderName, $"{invoiceId}.xml"));
            if (file.Exists)
                file.Delete();
        }

        public bool CanInvoiceBeDeleted(int orderId)
        {
            var utcNow = DateTime.UtcNow;
            var lastInvoiceNumber = GetLastInvoiceNumber(utcNow.Year);
            return _db.FEInvoices().Any(x => x.OrderId == orderId && x.Number == lastInvoiceNumber && x.Year == utcNow.Year && !x.HasXmlFile);
        }

        public IEnumerable<Invoice> GetAllInvoicesToCreateXml()
        {
            var fromDate = DateTime.UtcNow.AddHours(-24);
            return _db.FEInvoices()
                .AsNoTracking()
                .Where(x => x.CreatedOnUtc < fromDate)
                .Where(x => !x.HasXmlFile);
        }

        public bool CheckInvoiceForCustomerId(int customerId, int orderId)
        {
            var order = _db.Orders.Find(orderId);
            if (order != null)
                return order.CustomerId == customerId;
            return false;
        }

        public void NormalizeAddresses()
        {
            _db.Database.ExecuteSqlRaw("UPDATE [Address] SET Salutation=CASE WHEN Company IS NULL THEN 'P' ELSE 'A' END WHERE Salutation IS NULL");
            _db.Database.ExecuteSqlRaw("UPDATE [Address] SET Salutation='P' WHERE Salutation='Individual' OR Salutation='Consumer' OR Salutation='Persona fisica'");
            _db.Database.ExecuteSqlRaw("UPDATE [Address] SET Salutation='A' WHERE Salutation='Company' OR Salutation='Azienda'");
        }

        #endregion

        #region History

        public IEnumerable<InvoiceHistory> GetInvoiceHistoriesByInvoiceId(int invoiceId)
            => _db.FEInvoiceHistories().Where(x => x.InvoiceId == invoiceId);

        public void InsertInvoiceHistory(InvoiceHistory item)
        {
            _db.FEInvoiceHistories().Add(item);
            _db.SaveChanges();
        }

        public InvoiceHistory GetLastInvoiceHistoryWithFileNameByInvoiceId(int invoiceId)
            => _db.FEInvoiceHistories()
                .Include(x => x.Invoice)
                .Where(x => x.InvoiceId == invoiceId)
                .Where(x => x.SdiFileName != null)
                .OrderByDescending(x => x.CreatedOnUtc)
                .FirstOrDefault();

        #endregion
    }
}
