using Bumbo.Models;

namespace Bumbo.ViewModels
{
    public class UrenOverzichtMedewerkerViewModel
    {
        public DateTime? shiftDate;
        public string departmentName;
        public DateTime startTime;
        public DateTime endTime;
        public TimeSpan? registeredStartTime;
        public TimeSpan? registeredEndTime;
        public List<string> textColors = new List<string>();
        public int breakTime;
        public int Year { get; set; }
        public int WeekNumber { get; set; }
        public int previousWeek { get; set; }
        public int nextWeek { get; set; }
        public DateTime FirstDayOfSelectedWeek { get; set; }
        public DateTime LastDayOfSelectedWeek { get; set; }

        public List<Diensten> diensten = new List<Diensten>();
    }
}
