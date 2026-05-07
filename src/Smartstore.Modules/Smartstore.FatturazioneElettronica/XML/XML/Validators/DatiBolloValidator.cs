using FluentValidation;
using Smartstore.FatturazioneElettronica.XML.FatturaElettronicaBody.DatiGenerali;

namespace Smartstore.FatturazioneElettronica.XML.Validators
{
    public class DatiBolloValidator : AbstractValidator<DatiBollo>
    {
        public DatiBolloValidator()
        {
            RuleFor(x => x.BolloVirtuale)
                .NotEmpty()
                .Equal("SI");
            RuleFor(x => x.ImportoBollo)
                .NotNull();
        }
    }
}
