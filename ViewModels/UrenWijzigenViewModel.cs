using Bumbo.Models;
using System.ComponentModel.DataAnnotations;

namespace Bumbo.ViewModels
{
    public class UrenWijzigenViewModel
    {
        public int Dienst_Id { get; set; }
        public required string Name { get; set; }
        public required string Date { get; set; }
        public required string DayName { get; set; }
        public required string FromPage { get; set; }
        public int Year { get; set; }
        public int Week { get; set; }
        public TimeSpan ScheduledStart { get; set; }
        public TimeSpan ScheduledEnd { get; set; }
        public TimeSpan ActualStart { get; set; }
        public TimeSpan ActualEnd { get; set; }
        public TimeSpan BreakLength { get; set; }
        public TimeOnly? PauseStart1 { get; set; }
        public TimeOnly? PauseEnd1 { get; set; }
        public TimeOnly? PauseStart2 { get; set; }
        public TimeOnly? PauseEnd2 { get; set; }
        public bool IsSick { get; set; }

    }
}