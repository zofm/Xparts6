using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Validators;

namespace Smartstore.FatturazioneElettronica.XML.Validators
{
    public abstract class LatinBaseValidator<T> : PropertyValidator<T, string>
    {
        private readonly Charsets _charset;

        public LatinBaseValidator(Charsets charset)
        {
            _charset = charset;
        }

        public override string Name => "LatinBaseValidator";

        public override bool IsValid(ValidationContext<T> context, string value)
        {
            if (value == null || value == string.Empty) return true;

            var challenge = _charset == Charsets.BasicLatin
                ? @"^[\p{IsBasicLatin}]+$"
                : @"^[\p{IsBasicLatin}\p{IsLatin-1Supplement}]+$";

            return Regex.Match(value, challenge).Success;
        }

        protected override string GetDefaultMessageTemplate(string errorCode)
            => $"Testo contentente caratteri non validi ({(_charset == Charsets.BasicLatin ? "Unicode Basic Latin" : "Unicode Latin-1 Supplement")})";
    }

    public class BasicLatinValidator<T> : LatinBaseValidator<T>
    {
        public BasicLatinValidator() : base(Charsets.BasicLatin) { }
    }

    public class Latin1SupplementValidator<T> : LatinBaseValidator<T>
    {
        public Latin1SupplementValidator() : base(Charsets.Latin1Supplement) { }
    }

    public enum Charsets { BasicLatin, Latin1Supplement };

    public static class MyValidatorExtensions
    {
        public static IRuleBuilderOptions<T, string> BasicLatinValidator<T>(this IRuleBuilder<T, string> ruleBuilder)
            => ruleBuilder.SetValidator(new BasicLatinValidator<T>());

        public static IRuleBuilderOptions<T, string> Latin1SupplementValidator<T>(this IRuleBuilder<T, string> ruleBuilder)
            => ruleBuilder.SetValidator(new Latin1SupplementValidator<T>());
    }
}
