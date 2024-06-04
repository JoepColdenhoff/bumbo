using Bumbo.Models;

namespace Bumbo.ViewModels
{
    public class BeschikbaarheidOverviewViewModel
    {
        public int MedewerkerId { get; set; }

        public int Year { get; set; }

        public int WeekNumber { get; set; }

        public bool IsDienst { get; set; }

        public bool IsUsed { get; set; }

        public Dictionary<DateTime, List<Beschikbaarheid>> BeschikbaarheidList { get; set; }

    }
}
