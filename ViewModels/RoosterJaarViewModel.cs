using Bumbo.Models;

namespace Bumbo.ViewModels
{
    public class RoosterJaarViewModel
    {
        public int Year { get; set; }

        public int CurrentWeek { get; set; }

        public List<WeekGroup> MergedData { get; set; }

        public bool PreviousRoosterYear {  get; set; }
    }
}