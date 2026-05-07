using FluentValidation;
using Smartstore.FatturazioneElettronica.XML.Common;

namespace Smartstore.FatturazioneElettronica.XML.Validators
{
    public class DatiAnagraficiTerzoIntermediarioValidator : AbstractValidator<DatiAnagrafici>
    {
        public DatiAnagraficiTerzoIntermediarioValidator()
        {
            RuleFor(x => x.IdFiscaleIVA)
                .SetValidator(new IdFiscaleIVAValidator())
                .When(x=>!x.IdFiscaleIVA.IsEmpty());
            RuleFor(x => x.CodiceFiscale)
                .Length(11, 16)
                .When(x => !string.IsNullOrEmpty(x.CodiceFiscale));
            RuleFor(x => x.Anagrafica)
                .SetValidator(new AnagraficaValidator());
        }
    }
}
