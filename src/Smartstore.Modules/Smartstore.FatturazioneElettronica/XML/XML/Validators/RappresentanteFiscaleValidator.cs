using Smartstore.FatturazioneElettronica.XML.FatturaElettronicaHeader.RappresentanteFiscale;
using FluentValidation;

namespace Smartstore.FatturazioneElettronica.XML.Validators
{
    public class RappresentanteFiscaleValidator : AbstractValidator<RappresentanteFiscale>
    {
        public RappresentanteFiscaleValidator()
        {
            RuleFor(x => x.DatiAnagrafici)
                .SetValidator(new DatiAnagraficiRappresentanteFiscaleValidator());
        }
    }
}
