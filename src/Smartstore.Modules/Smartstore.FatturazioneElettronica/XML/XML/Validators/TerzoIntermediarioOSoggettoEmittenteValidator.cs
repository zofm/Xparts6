using Smartstore.FatturazioneElettronica.XML.FatturaElettronicaHeader.TerzoIntermediarioOSoggettoEmittente;
using FluentValidation;

namespace Smartstore.FatturazioneElettronica.XML.Validators
{
    public class TerzoIntermediarioOSoggettoEmittenteValidator : AbstractValidator<TerzoIntermediarioOSoggettoEmittente>
    {
        public TerzoIntermediarioOSoggettoEmittenteValidator()
        {
            RuleFor(x => x.DatiAnagrafici)
                .SetValidator(new DatiAnagraficiTerzoIntermediarioValidator());
        }
    }
}
