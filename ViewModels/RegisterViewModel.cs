using Bumbo.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Bumbo.ViewModels
{
    public class RegisterViewModel
    {

        public int Year { get; set; }
        public int WeekNumber { get; set; }
        public Dictionary<DateTime, string> DayOptions { get; set; }
        public List<Afdelingen> Afdelingens { get; set; }

        public DateTime FirstDayOfCurrentWeek { get; set; }
        public DateTime CurrentDay { get; set; }

        public string PreviousWeekUrl { get; set; }
        public string NextWeekUrl { get; set; }

        public List<string> AfdelingNamen { get; set; }
        public bool AllApproved { get; set; }
        public string CurrentDayName { get; set; }

        public List<RegisterViewModelItem> DienstenViewModel { get; set; }
        public Dictionary<DateTime, bool> ButtonStatuses { get; set; }

    }
}
