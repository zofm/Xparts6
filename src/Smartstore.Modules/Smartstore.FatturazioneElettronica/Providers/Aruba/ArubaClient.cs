using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Smartstore.FatturazioneElettronica.Providers.Aruba.Common;
using Smartstore.FatturazioneElettronica.Providers.Aruba.Models;
using Smartstore.FatturazioneElettronica.Providers.Models;

namespace Smartstore.FatturazioneElettronica.Providers.Aruba
{
    public class ArubaClient : IFatturazioneElettronicaClient
    {
        private ArubaAuthentication _authClient;

        private readonly string _baseUrl;
        private readonly string _baseAuthUrl;
        private readonly string _username;
        private readonly string _password;

        public ArubaClient(string baseUrl, string baseAuthUrl, string username, string password)
        {
            _baseUrl = baseUrl;
            _baseAuthUrl = baseAuthUrl;
            _username = username;
            _password = password;
            _authClient = new ArubaAuthentication(baseAuthUrl, username, password);
        }

        public UploadUnsignedInvoiceResponse UploadUnsignedInvoice(UploadUnsignedInvoiceRequest req)
        {
            var response = new UploadUnsignedInvoiceResponse();

            string fileBase64Data;
            try
            {
                fileBase64Data = Utility.GetBase64EncodedXML(req.XmlFilePath);
            }
            catch (Exception e)
            {
                response.ErrorCode = "FCF";
                response.ErrorDescription = string.Format("Impossibile caricare il file XML ({0})", e.Message);
                return response;
            }

            var arubaReq = new UploadInvoiceRequest
            {
                DataFile = fileBase64Data,
                Credentials = string.Empty,
                Domain = string.Empty
            };

            try
            {
                string postData = JsonConvert.SerializeObject(arubaReq);
            }
            catch (Exception e)
            {
                response.ErrorCode = "SER";
                response.ErrorDescription = string.Format("Impossibile serializzare il file XML ({0})", e.Message);
                return response;
            }

            try
            {
                var wrapper = Post<UploadInvoiceResponse, UploadInvoiceRequest>(arubaReq, _baseUrl + Constants.UploadInvoicePath);
                if (wrapper.StatusCode == HttpStatusCode.Forbidden)
                {
                    _authClient.ForceRefreshToken = true;
                    wrapper = Post<UploadInvoiceResponse, UploadInvoiceRequest>(arubaReq, _baseUrl + Constants.UploadInvoicePath);
                    if (wrapper.StatusCode == HttpStatusCode.Forbidden)
                    {
                        response.StatusCode = HttpStatusCode.Forbidden;
                        return response;
                    }
                }

                response.StatusCode = wrapper.StatusCode;
                response.ErrorCode = wrapper.Response?.ErrorCode;
                response.ErrorDescription = wrapper.Response?.ErrorDescription;
                response.FileName = wrapper.Response?.UploadFileName;
            }
            catch (WebException wex)
            {
                response.ErrorCode = "WEX";
                response.ErrorDescription = string.Format("Impossibile inviare la fattura (eccezione WEB: {0})", wex.Message);
            }
            catch (Exception e)
            {
                response.ErrorCode = "SEF";
                response.ErrorDescription = string.Format("Impossibile inviare la fattura ({0})", e.Message);
            }

            return response;
        }

        public DownloadUnsignedInvoiceResponse DownloadUnsignedInvoice(DownloadUnsignedInvoiceRequest req)
        {
            var response = new DownloadUnsignedInvoiceResponse();

            if (string.IsNullOrEmpty(req.FileName))
            {
                response.ErrorCode = "NFA";
                response.ErrorDescription = "Nome file necessario.";
                return response;
            }

            try
            {
                var url = $"{_baseUrl}{Constants.DownloadInvoicePath}?filename={req.FileName}&includePdf=true&includeFile=false";
                var wrapper = Get<GetInvoiceByFileNameResponse>(url);
                if (wrapper.StatusCode == HttpStatusCode.Forbidden)
                {
                    _authClient.ForceRefreshToken = true;
                    wrapper = Get<GetInvoiceByFileNameResponse>(url);
                    if (wrapper.StatusCode == HttpStatusCode.Forbidden)
                    {
                        response.StatusCode = HttpStatusCode.Forbidden;
                        return response;
                    }
                }

                response.StatusCode = wrapper.StatusCode;
                response.ErrorCode = wrapper.Response?.ErrorCode;
                response.ErrorDescription = wrapper.Response?.ErrorDescription;
                response.Id = wrapper.Response?.Id;
                response.AziendaInviante = new Azienda
                {
                    CodiceFiscale = wrapper.Response?.Sender?.FiscalCode,
                    CodicePaese = wrapper.Response?.Sender?.CountryCode,
                    PartitaIva = wrapper.Response?.Sender?.VatCode,
                    Descrizione = wrapper.Response?.Sender?.Description,
                };
                response.AziendaRicevente = new Azienda
                {
                    CodiceFiscale = wrapper.Response?.Receiver?.FiscalCode,
                    CodicePaese = wrapper.Response?.Receiver?.CountryCode,
                    PartitaIva = wrapper.Response?.Receiver?.VatCode,
                    Descrizione = wrapper.Response?.Receiver?.Description,
                };
                response.TipoFattura = wrapper.Response?.InvoiceType;
                response.TipoDocumento = wrapper.Response?.DocType;
                response.File = wrapper.Response?.File;
                response.PdfFile = wrapper.Response?.PdfFile;
                response.NomeFile = wrapper.Response?.FileName;
                response.Fatture = wrapper.Response?.Invoices?.Select(x => new Fattura
                {
                    Stato = x.Status,
                    Data = x.InvoiceDate,
                    Numero = x.Number,
                }).ToList();
                response.Username = wrapper.Response?.Username;
                response.DataUltimoAggiornamento = wrapper.Response?.LastUpdate;
                response.IdSDI = wrapper.Response?.IdSDI;
            }
            catch (WebException wex)
            {
                response.ErrorCode = "WEX";
                response.ErrorDescription = string.Format("Impossibile recuperare le info per il file {0} (eccezione WEB: {1})", req.FileName, wex.Message);
            }
            catch (Exception e)
            {
                response.ErrorCode = "IRI";
                response.ErrorDescription = string.Format("Impossibile recuperare le info per il file {0} ({1})", req.FileName, e.Message);
            }

            return response;
        }

        public BasePOSTResponse<RES> Post<RES, PAR>(PAR pars, string url)
        {
            var postData = JsonConvert.SerializeObject(pars);
            var bytes = Encoding.UTF8.GetBytes(postData);
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentLength = bytes.Length;
            httpWebRequest.ContentType = "application/json;charset=UTF-8";
            httpWebRequest.Accept = "application/json";
            httpWebRequest.Headers.Add("Authorization", "Bearer " + _authClient.AccessToken);

            using (Stream requestStream = httpWebRequest.GetRequestStream())
                requestStream.Write(bytes, 0, bytes.Length);

            var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var reader = new StreamReader(httpWebResponse.GetResponseStream()))
            {
                var objText = reader.ReadToEnd();
                return new BasePOSTResponse<RES>
                {
                    Response = JsonConvert.DeserializeObject<RES>(objText),
                    StatusCode = httpWebResponse.StatusCode
                };
            }
        }

        public BaseGETResponse<RES> Get<RES>(string url)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest.Method = "GET";
            httpWebRequest.ContentType = "application/json;charset=UTF-8";
            httpWebRequest.Accept = "application/json";
            httpWebRequest.Headers.Add("Authorization", "Bearer " + _authClient.AccessToken);

            var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var reader = new StreamReader(httpWebResponse.GetResponseStream()))
            {
                var objText = reader.ReadToEnd();
                return new BaseGETResponse<RES>
                {
                    Response = JsonConvert.DeserializeObject<RES>(objText),
                    StatusCode = httpWebResponse.StatusCode
                };
            }
        }
    }
}
