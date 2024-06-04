using Bumbo.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using static Bumbo.ViewModels.RoosterViewStructs;

namespace Bumbo.ViewModels
{
    public class RoosteringViewModel
    {
        public int year { get; set; }
        public int WeekNumber { get; set; }
        public DateTime SelectedDate { get; set; }
        public List<DayOption> DayOptions { get; set; }
        public List<DepartmentOption> DepartmentOptions { get; set; }
        public int SelectedDepartment { get; set; }
        public int addToRosterId { get; set; }
        public List<Beschikbaarheid> AvailableAvailabilities { get; set; } = new List<Beschikbaarheid>();
        public List<Diensten> ScheduledAvailabilities { get; set; } = new List<Diensten>();
        public int PrognoseUren { get; set; }
        public TimeSpan scheduledHours { get; set; }
    }
}
