namespace Bumbo.ViewModels
{
    public class MaandRegisterDataViewModel
    {
        public List<MaandRegisterViewModel>? maandRegisterViewModels { get; set; }
        public int Year { get; set; }
        public int CurrentMonthNumber { get; set; }
        public string CurrentMonthName { get; set; }
        public string PreviousMonthUrl { get; set; }
        public string NextMonthUrl { get; set; }
        public List<string> AfdelingNamen { get; set; }
        public string CurrentAfdeling { get; set; }
    }
}
