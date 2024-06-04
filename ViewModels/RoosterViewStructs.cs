namespace Bumbo.ViewModels
{
    public class RoosterViewStructs
    {
        public struct DayOption
        {
            public DateTime Date { get; set; }
            public string Display { get; set; }
        }

        public struct DepartmentOption
        {
            public int DepartmentId { get; set; }
            public string Name { get; set; }
        }

    }
}
