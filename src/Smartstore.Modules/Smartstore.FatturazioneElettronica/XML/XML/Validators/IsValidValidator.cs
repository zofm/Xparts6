using System;
using FluentValidation;
using FluentValidation.Validators;
using Smartstore.FatturazioneElettronica.XML.Tabelle;

namespace Smartstore.FatturazioneElettronica.XML.Validators
{
    public class IsValidValidator<TModel, T> : PropertyValidator<TModel, string>
        where T : Tabella, new()
    {
        private readonly string[] _domain = new T().Codici;

        public override string Name => "IsValidValidator";

        public override bool IsValid(ValidationContext<TModel> context, string value)
        {
            context.MessageFormatter.AppendArgument("AcceptedValues", string.Join(", ", _domain));
            return value == null || Array.IndexOf(_domain, value) != -1;
        }

        protected override string GetDefaultMessageTemplate(string errorCode)
            => "'{PropertyName}' valori accettati: {AcceptedValues}";
    }
}
