using System;
using System.ComponentModel.DataAnnotations;

namespace Bumbo.ViewModels
{
    public class DateRangeAttribute : ValidationAttribute
    {
        private readonly int _minYearsAgo;
        private readonly int _maxYearsAgo;

        public DateRangeAttribute(int minYearsAgo, int maxYearsAgo)
        {
            _minYearsAgo = minYearsAgo;
            _maxYearsAgo = maxYearsAgo;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            DateTime date = Convert.ToDateTime(value);

            // Calculate the minimum and maximum allowed birthdates
            DateTime minBirthdate = DateTime.Now.AddYears(-_maxYearsAgo);
            DateTime maxBirthdate = DateTime.Now.AddYears(-_minYearsAgo);

            if (date < minBirthdate || date > maxBirthdate)
            {
                return new ValidationResult($"De geboortedatum moet tussen {maxBirthdate:dd-MM-yyyy} en {minBirthdate:dd-MM-yyyy} liggen.");
            }

            return ValidationResult.Success;
        }
    }
}
