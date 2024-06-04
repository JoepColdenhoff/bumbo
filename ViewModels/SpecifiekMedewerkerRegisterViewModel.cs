namespace Bumbo.ViewModels
{
    public class SpecifiekMedewerkerRegisterViewModel
    {
        public int DienstId {  get; set; }
        public bool IsApproved { get; set; }
        public bool HasDeviation { get; set; }
        public string Datum {  get; set; }
        public string Afdeling { get; set; }
        public string Gepland { get; set; }
        public string Geklokt { get; set; }
        public int Pauze { get; set; }
        public double Toeslag_0 { get; set; }
        public double Toeslag_33 { get; set; }
        public double Toeslag_50 { get; set; }
        public double Toeslag_70 { get; set; }
        public double Toeslag_100 { get; set; }      

    }
}
