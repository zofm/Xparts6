using FluentValidation;
using Smartstore.FatturazioneElettronica.XML.FatturaElettronicaBody.DatiGenerali;
using Smartstore.FatturazioneElettronica.XML.Tabelle;

namespace Smartstore.FatturazioneElettronica.XML.Validators
{
    public class DatiRitenutaValidator : AbstractValidator<DatiRitenuta>
    {
        public DatiRitenutaValidator()
        {
            RuleFor(x => x.TipoRitenuta)
                .NotEmpty()
                .SetValidator(new IsValidValidator<DatiRitenuta, TipoRitenuta>());
            RuleFor(x => x.ImportoRitenuta)
                .NotNull();
            RuleFor(x => x.AliquotaRitenuta)
                .NotNull();
            RuleFor(x => x.CausalePagamento)
                .NotEmpty()
                .SetValidator(new IsValidValidator<DatiRitenuta, CausalePagamento>());
        }
    }
}
