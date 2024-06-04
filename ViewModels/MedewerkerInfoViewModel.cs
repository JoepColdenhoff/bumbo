using Bumbo.Models;
using System.ComponentModel.DataAnnotations;

namespace Bumbo.ViewModels
{
    public class MedewerkerInfoViewModel
    {
        public int MedewerkerId { get; set; }

        public string Voornaam { get; set; } = null!;

        public string? Tussenvoegsel { get; set; }

        public string Achternaam { get; set; } = null!;

        public List<Afdelingen> Afdelingen { get; set; }

        public string Email { get; set; } = null!;

        public string Telefoonnummer { get; set; } = null!;

        public int Schaal { get; set; }

        public string Geboortedatum { get; set; }

        public int Leeftijd { get; set; }

        public string Postcode { get; set; } = null!;

        public string Huisnummer { get; set; }

        public string Straatnaam { get; set; } = null!;

        public string Plaats { get; set; } = null!;
       
        public string Indienst { get; set; }

        public bool Verwijdert { get; set; }

        public virtual Account EmailNavigation { get; set; } = null!;

        public virtual Filialen? Filiaal { get; set; }

        public virtual Functie? Functie { get; set; }

    }

}

