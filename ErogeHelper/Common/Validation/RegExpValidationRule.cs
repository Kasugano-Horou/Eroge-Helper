using log4net;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace ErogeHelper.Common.Validation
{
    class RegExpValidationRule : ValidationRule
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(RegExpValidationRule));

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string pattern = value.ToString();
            RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Compiled;

            try
            {
                Regex optionRegex = new Regex(pattern, options);
            }
            catch(ArgumentException ex)
            {
                log.Info("Check regexp format, it's fine exception");
                return new ValidationResult(false, $"Invalid RegExp. {ex.Message}");
            }

            return ValidationResult.ValidResult;
        }
    }
}
