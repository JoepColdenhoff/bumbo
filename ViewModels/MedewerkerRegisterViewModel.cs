using Bumbo.Models;

namespace Bumbo.ViewModels
{
    public class MedewerkerRegisterViewModel
    {
        public List<SpecifiekMedewerkerRegisterViewModel> Rijen { get; set; }

        public Medewerker Medewerker { get; set; }
        public int CurrentMonthNumber { get; set; }
        public int Year { get; set; }
        public string CurrentMonthName {  get; set; }
        public string CurrentAfdeling {  get; set; }
    }
}
