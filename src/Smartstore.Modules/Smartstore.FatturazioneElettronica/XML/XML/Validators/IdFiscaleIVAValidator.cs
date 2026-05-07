using Smartstore.FatturazioneElettronica.XML.Common;
using Smartstore.FatturazioneElettronica.XML.Tabelle;
using FluentValidation;

namespace Smartstore.FatturazioneElettronica.XML.Validators
{
    public class IdFiscaleIVAValidator : AbstractValidator<IdFiscaleIVA>
    {
        public IdFiscaleIVAValidator()
        {
            RuleFor(id => id.IdPaese)
                .NotEmpty()
                .SetValidator(new IsValidValidator<IdFiscaleIVA, IdPaese>());
            RuleFor(id => id.IdCodice)
                .NotEmpty()
                .Length(1, 28);
        }
    }
}
