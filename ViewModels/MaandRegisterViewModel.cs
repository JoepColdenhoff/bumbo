using Bumbo.Models;

namespace Bumbo.ViewModels
{
    public class MaandRegisterViewModel
    {
        public Medewerker Medewerker { get; set; }
        public string Functie { get; set; }
        public double TotaalUren { get; set; }
        public double ToeslagUur0 { get; set; }
        public double ToeslagUur33 { get; set; }
        public double ToeslagUur50 { get; set; }
        public double ToeslagUur70 { get; set; }
        public double ToeslagUur100 { get; set; }
    }
}
