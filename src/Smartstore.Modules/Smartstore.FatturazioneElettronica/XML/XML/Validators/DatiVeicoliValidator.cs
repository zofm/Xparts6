using FluentValidation;
using Smartstore.FatturazioneElettronica.XML.FatturaElettronicaBody.DatiVeicoli;

namespace Smartstore.FatturazioneElettronica.XML.Validators
{
    public class DatiVeicoliValidator : AbstractValidator<DatiVeicoli>
    {
        public DatiVeicoliValidator()
        {
            RuleFor(x => x.Data)
                .NotNull();
            RuleFor(x => x.TotalePercorso)
                .NotEmpty()
                .BasicLatinValidator()
                .Length(1, 15);
        }
    }
}
