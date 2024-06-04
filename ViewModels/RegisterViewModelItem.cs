namespace Bumbo.ViewModels
{
    public class RegisterViewModelItem
    {
        public int DienstId { get; set; }
        public string MedewerkerNaam { get; set; }
        public TimeSpan StartTijd { get; set; }
        public TimeSpan EindTijd { get; set; }
        public string DienstTijden { get; set; }
        public string InklokTijden { get; set; }
        public bool IsApproved { get; set; }

        public bool HasDeviation { get; set; }
        public int Pauze { get; set; }

        public double Toeslag_0 { get; set; }
        public double Toeslag_33 { get; set; }
        public double Toeslag_50 { get; set; }
        public double UrenZiek { get; set; }
        public double Toeslag_100 { get; set; }
    }
}
