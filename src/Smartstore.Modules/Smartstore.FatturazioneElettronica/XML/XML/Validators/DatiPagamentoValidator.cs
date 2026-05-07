using FluentValidation;
using Smartstore.FatturazioneElettronica.XML.FatturaElettronicaBody.DatiPagamento;
using Smartstore.FatturazioneElettronica.XML.Tabelle;

namespace Smartstore.FatturazioneElettronica.XML.Validators
{
    public class DatiPagamentoValidator : AbstractValidator<DatiPagamento>
    {
        public DatiPagamentoValidator()
        {
            RuleFor(x => x.CondizioniPagamento)
                .NotEmpty()
                .SetValidator(new IsValidValidator<DatiPagamento, Tabelle.CondizioniPagamento>());
            RuleForEach(x => x.DettaglioPagamento)
                .SetValidator(new DettaglioPagamentoValidator());
            RuleFor(x => x.DettaglioPagamento)
                .NotEmpty();
        }
    }
}
