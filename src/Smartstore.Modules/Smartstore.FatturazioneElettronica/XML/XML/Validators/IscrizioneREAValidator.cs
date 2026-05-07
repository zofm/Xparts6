using Smartstore.FatturazioneElettronica.XML.FatturaElettronicaHeader.CedentePrestatore;
using Smartstore.FatturazioneElettronica.XML.Tabelle;
using FluentValidation;

namespace Smartstore.FatturazioneElettronica.XML.Validators
{
    public class IscrizioneREAValidator : AbstractValidator<IscrizioneREA>
    {
        public IscrizioneREAValidator()
        {
            RuleFor(x => x.Ufficio)
                .NotEmpty()
                .SetValidator(new IsValidValidator<IscrizioneREA, Provincia>());
            RuleFor(x => x.NumeroREA)
                .NotEmpty()
                .BasicLatinValidator()
                .Length(1, 20);
            RuleFor(x => x.SocioUnico)
                .SetValidator(new IsValidValidator<IscrizioneREA, SocioUnico>())
                .When(x => !string.IsNullOrEmpty(x.SocioUnico));
            RuleFor(x => x.StatoLiquidazione)
                .NotEmpty()
                .SetValidator(new IsValidValidator<IscrizioneREA, StatoLiquidazione>());
        }
    }
}
