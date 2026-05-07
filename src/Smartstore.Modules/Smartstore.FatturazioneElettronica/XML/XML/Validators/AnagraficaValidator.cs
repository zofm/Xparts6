using FluentValidation;
using Smartstore.FatturazioneElettronica.XML.Common;
namespace Smartstore.FatturazioneElettronica.XML.Validators
{
    public  class AnagraficaValidator : DenominazioneNomeCognomeValidator<Anagrafica>
    {
        public AnagraficaValidator()
        {
            RuleFor(x => x.Titolo)
                .Length(2, 10)
                .BasicLatinValidator()
                .When(x=>!string.IsNullOrEmpty(x.Titolo));
            RuleFor(x => x.CodEORI)
                .Length(13, 17)
                .When(x => !string.IsNullOrEmpty(x.CodEORI));
        }
    }
}
