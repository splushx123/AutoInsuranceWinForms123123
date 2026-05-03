using System;
using System.Text.RegularExpressions;

namespace AutoInsuranceWinForms
{
    public static class ValidationRules
    {
        public static bool IsOlderThanYears(DateTime birthDate, int years)
        {
            var today = DateTime.Today;
            return birthDate.Date < today.AddYears(-years);
        }

        public static bool TryNormalizePhoneRu(string phoneInput, out string normalizedPhone)
        {
            var digits = Regex.Replace(phoneInput ?? string.Empty, @"\D", string.Empty);
            normalizedPhone = string.Empty;

            if (digits.Length == 10)
                digits = "7" + digits;

            if (digits.Length != 11 || !digits.StartsWith("7"))
                return false;

            normalizedPhone = "+7" + digits.Substring(1);
            return true;
        }
    }
}
