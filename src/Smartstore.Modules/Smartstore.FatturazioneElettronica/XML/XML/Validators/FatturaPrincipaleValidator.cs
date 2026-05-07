using FluentValidation;
using Smartstore.FatturazioneElettronica.XML.FatturaElettronicaBody.DatiGenerali;

namespace Smartstore.FatturazioneElettronica.XML.Validators
{
    public class FatturaPrincipaleValidator : AbstractValidator<FatturaPrincipale>
    {
        public FatturaPrincipaleValidator()
        {
            RuleFor(x => x.NumeroFatturaPrincipale)
                .NotEmpty()
                .BasicLatinValidator()
                .Length(1, 20);
            RuleFor(x => x.DataFatturaPrincipale)
                .NotNull();
        }
    }
}
