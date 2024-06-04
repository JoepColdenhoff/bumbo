using Bumbo.Models;
using System.ComponentModel.DataAnnotations;

namespace Bumbo.ViewModels
{
    public class MedewerkerFormViewModel
    {
        public List<Functie> Functies { get; set; }

        public int MedewerkerId { get; set; }

        public int? FunctieId { get; set; }

        public int? FiliaalId { get; set; }

        [Required(ErrorMessage = "Voornaam is verplicht.")]
        public string Voornaam { get; set; } = null!;

        [StringLength(45, ErrorMessage = "Tussenvoegsel mag maximaal 45 tekens bevatten.")]
        public string? Tussenvoegsel { get; set; }

        [Required(ErrorMessage = "Achternaam is verplicht.")]
        [StringLength(45, ErrorMessage = "Achternaam mag maximaal 45 tekens bevatten.")]
        public string Achternaam { get; set; } = null!;

        [Required(ErrorMessage = "Email is verplicht.")]
        [StringLength(45, ErrorMessage = "Email mag maximaal 45 tekens bevatten.")]
        [EmailAddress(ErrorMessage = "Ongeldig e-mailadres.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Telefoonnummer is verplicht.")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Ongeldig telefoonnummer formaat.")]
        public string Telefoonnummer { get; set; } = null!;

        [Required(ErrorMessage = "Geboortedatum is verplicht.")]
        [DataType(DataType.Date, ErrorMessage = "Ongeldige geboortedatum.")]
        [DateRange(100, 15, ErrorMessage = "De geboortedatum moet tussen 100 jaar geleden en 15 jaar geleden liggen.")]
        public DateTime Geboortedatum { get; set; } = new DateTime(DateTime.Now.Year -100, 1, 1);

        [Required(ErrorMessage = "Postcode is verplicht.")]
        [StringLength(6, ErrorMessage = "Postcode mag maximaal 6 tekens bevatten.")]
        [RegularExpression(@"^[1-9][0-9]{3} ?[A-Za-z]{2}$", ErrorMessage = "Ongeldig postcode formaat.")]
        public string Postcode { get; set; } = null!;

        [Required(ErrorMessage = "Huisnummer is verplicht.")]
        public string Huisnummer { get; set; }

        [Required(ErrorMessage = "Straatnaam is verplicht.")]
        public string Straatnaam { get; set; } = null!;

        [Required(ErrorMessage = "Plaats is verplicht.")]
        public string Plaats { get; set; } = null!;

        [Required(ErrorMessage = "Indienst is verplicht.")]
        [DataType(DataType.Date, ErrorMessage = "Ongeldige indienstdatum.")]
        [DateRange(100, 0, ErrorMessage = "De in dienst datum moet tussen 100 jaar geleden en vandaag liggen.")]
        public DateTime Indienst { get; set; } = new DateTime(DateTime.Now.Year - 100, 1, 1);

        public virtual Account EmailNavigation { get; set; } = null!;

        public virtual Filialen? Filiaal { get; set; }

        public virtual Functie? Functie { get; set; }
    }
}
