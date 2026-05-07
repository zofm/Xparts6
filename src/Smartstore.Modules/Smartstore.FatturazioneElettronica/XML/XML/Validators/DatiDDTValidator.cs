using FluentValidation;
using Smartstore.FatturazioneElettronica.XML.FatturaElettronicaBody.DatiGenerali;

namespace Smartstore.FatturazioneElettronica.XML.Validators
{
    public class DatiDDTValidator : AbstractValidator<DatiDDT>
    {
        public DatiDDTValidator()
        {
            RuleFor(x => x.NumeroDDT)
                .NotEmpty()
                .BasicLatinValidator()
                .Length(1, 20);
        }
    }
}
