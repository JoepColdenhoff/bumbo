using Bumbo.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using static Bumbo.ViewModels.RoosterViewStructs;

namespace Bumbo.ViewModels
{
    public class RoosterOverviewViewModel
    {
        public List<DayOption> DayOptions { get; set; }
        public int WeekNumber { get; set; }
        public int year { get; set; }
        public DateTime SelectedDate { get; set; }
        public List<Diensten> Diensten { get; set; }
        public List<Afdelingen> Afdelingen { get; set; }
        public List<Medewerker> Medewerkers { get; set; }

    }
}