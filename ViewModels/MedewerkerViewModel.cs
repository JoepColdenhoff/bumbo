using Bumbo.Models;
using System.ComponentModel.DataAnnotations;

namespace Bumbo.ViewModels
{
    public class MedewerkerViewModel
    {
        public List<Medewerker> medewerkers { get; set; }

        public string Naam { get; set; }

        public string Email { get; set; } = null!;

        public string Telefoonnummer { get; set; } = null!;

        public DateTime Geboortedatum { get; set; }

        public List<Afdelingen> Afdelingen { get; set; }

        public int AfdelingId { get; set; }

        public string Afdeling { get; set; }

        public virtual Filialen? Filiaal { get; set; }

        public virtual Functie? Functie { get; set; }
    }
}
