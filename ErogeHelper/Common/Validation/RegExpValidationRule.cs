using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace ErogeHelper.Common.Validation
{
    class RegExpValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string pattern = value.ToString();
            RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Compiled;

            try
            {
                Regex optionRegex = new Regex(pattern, options);
            }
            // 
            catch(ArgumentException ex)
            {
                return new ValidationResult(false, $"Invalid RegExp. {ex.Message}");
            }

            return ValidationResult.ValidResult;
        }
    }
}
