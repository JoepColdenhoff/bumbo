using Bumbo.Models;

namespace Bumbo.ViewModels
{
    public class BeschikbaarheidWeekViewModel
    {
        public int Year { get; set; }

        public int WeekNumber { get; set; }

        public int MedewerkerId { get; set; }

        public int CopyWeekNumber { get; set; }

        public List<TimeOnly> TimeOptions { get; set; }

        public List<int>? BeschikbaarheidOptions { get; set; }

        public required List<BeschikbaarheidViewModel> BeschikbaarheidList { get; set; }
    }
}