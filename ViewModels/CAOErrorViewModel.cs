namespace Bumbo.ViewModels
{
    public class CAOErrorViewModel
    {
        public bool TooManyHoursWithSchool { get; set; }
        public bool WorksAfter19 { get; set; }
        public bool WorkedOver40Hours { get; set; }
        public bool Over45Hours { get; set; }
        public bool Exceeds12Hours { get; set; }
        public bool Exceeds6HoursPerWeek { get; set; }
        public bool Exceeds12HoursSchoolWeek { get; set; }
        public bool Exceeds60Hours { get; set; }
        public bool Passed { get; set; }
    }
}
