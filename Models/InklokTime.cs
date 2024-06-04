using Bumbo.Validation;

namespace Bumbo.Models
{
    [InklokTimeValidation]
    public class InklokTime
    {
        public int InklokkenId { get; set; }

        public required TimeOnly Start { get; set; }

        public required TimeOnly End { get; set; }
    }
}