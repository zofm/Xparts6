using FluentValidation;

namespace Smartstore.FatturazioneElettronica.XML.Validators
{
    public class FatturaValidator : AbstractValidator<FatturaBase>
    {
        public FatturaValidator()
        {
            RuleFor(dt => dt.FatturaElettronicaHeader)
                .SetValidator(new FatturaElettronicaHeaderValidator());
            RuleForEach(dt => dt.FatturaElettronicaBody)
                .SetValidator(new FatturaElettronicaBodyValidator());
        }
    }
}
