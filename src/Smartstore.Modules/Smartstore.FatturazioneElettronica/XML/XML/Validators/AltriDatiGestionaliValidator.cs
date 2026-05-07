using FluentValidation;
using Smartstore.FatturazioneElettronica.XML.FatturaElettronicaBody.DatiBeniServizi;

namespace Smartstore.FatturazioneElettronica.XML.Validators
{
    public class AltriDatiGestionaliValidator : AbstractValidator<AltriDatiGestionali>
    {
        public AltriDatiGestionaliValidator()
        {
            RuleFor(x => x.TipoDato)
                .NotEmpty()
                .BasicLatinValidator()
                .Length(1, 10);
            RuleFor(x => x.RiferimentoTesto)
                .Length(1, 60)
                .Latin1SupplementValidator()
                .When(x => !string.IsNullOrEmpty(x.RiferimentoTesto));
        }
    }
}
