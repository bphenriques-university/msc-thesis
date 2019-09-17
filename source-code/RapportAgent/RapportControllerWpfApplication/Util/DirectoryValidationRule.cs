using System.Globalization;
using System.IO;
using System.Windows.Controls;

namespace RapportControllerWpfApplication.Util {
    class DirectoryExistsValidationRule : ValidationRule {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            string str = value as string;
            if (str == null)
                return new ValidationResult(false, "Bad type");

            if (Directory.Exists(str))
                return new ValidationResult(true, null);

            return new ValidationResult(false, "There is no such folder at " + str);
        }
    }
}
