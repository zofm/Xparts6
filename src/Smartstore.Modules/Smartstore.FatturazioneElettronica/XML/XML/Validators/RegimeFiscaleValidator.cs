using FluentValidation;
using FluentValidation.Validators;
using Smartstore.FatturazioneElettronica.XML.Tabelle;

namespace Smartstore.FatturazioneElettronica.XML.Validators
{
    public class RegimeFiscaleValidator<TModel, T> : IsValidValidator<TModel, T>
        where T : Tabella, new()
    {
        protected override string GetDefaultMessageTemplate(string errorCode)
        {
            if (errorCode == "00459")
                return "Regime fiscale RF03 non consentito per questo tipo di documento.";
            return base.GetDefaultMessageTemplate(errorCode);
        }
    }
}
