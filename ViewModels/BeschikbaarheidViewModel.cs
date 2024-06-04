using Bumbo.Models;

namespace Bumbo.ViewModels
{
    public class BeschikbaarheidViewModel
    {
        public int? BeschikbaarheidId { get; set; }

        public int? MedewerkerId { get; set; }

        public DateTime Datum { get; set; }

        public int? SchoolUren { get; set; }

        public TimeOnly StartTijd { get; set; }

        public TimeOnly EindTijd { get; set; }
    }
}
