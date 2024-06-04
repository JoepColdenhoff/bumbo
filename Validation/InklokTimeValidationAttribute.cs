using Bumbo.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Bumbo.Validation
{
    public class InklokTimeValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var inklokTimes = value as List<InklokTime>;
            if (inklokTimes == null)
            {
                return new ValidationResult("Invalid data.");
            }

            TimeOnly? lastEndTime = null;

            foreach (var inklokTime in inklokTimes)
            {
                if (inklokTime.Start >= inklokTime.End)
                {
                    return new ValidationResult("Starttijd moet eerder zijn dan eindtijd.");
                }

                if (lastEndTime.HasValue && inklokTime.Start <= lastEndTime)
                {
                    return new ValidationResult("Starttijd moet later zijn dan de laatste eindtijd.");
                }

                if ((inklokTime.End - inklokTime.Start) < TimeSpan.FromMinutes(15))
                {
                    return new ValidationResult("Er moet minstens 15 minuten pauze zijn tussen tijden.");
                }

                lastEndTime = inklokTime.End;

            }

            return ValidationResult.Success;
        }
    }
}