using Smartstore.FatturazioneElettronica.XML.Tabelle;
using FluentValidation;

namespace Smartstore.FatturazioneElettronica.XML.Validators
{
    public abstract class LocalitàBaseValidator<T> : AbstractValidator<T> where T : Common.Località
    {
        public LocalitàBaseValidator()
        {
            RuleFor(x => x.Indirizzo)
                .NotEmpty()
                .Length(1, 60)
                .Latin1SupplementValidator();
            RuleFor(x => x.NumeroCivico)
                .Length(1, 8)
                .When(x => !string.IsNullOrEmpty(x.NumeroCivico));
            RuleFor(x => x.CAP)
                .NotEmpty()
                .Length(5);
            RuleFor(x => x.Comune)
                .NotEmpty()
                .Length(1, 60)
                .Latin1SupplementValidator();
            RuleFor(x => x.Provincia)
                .SetValidator(new IsValidValidator<T, Provincia>())
                .When(x => !string.IsNullOrEmpty(x.Provincia));
            RuleFor(id => id.Nazione)
                .NotEmpty()
                .SetValidator(new IsValidValidator<T, IdPaese>());
        }
    }
}
