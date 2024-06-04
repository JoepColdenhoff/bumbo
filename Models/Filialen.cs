using NetTopologySuite.Geometries;

namespace Bumbo.Models;

public partial class Filialen
{
    public int FiliaalId { get; set; }

    public string Naam { get; set; } = null!;

    public string Postcode { get; set; } = null!;

    public string Huisnummer { get; set; } = null!;

    public string Straatnaam { get; set; } = null!;

    public string Plaats { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Telefoonnummer { get; set; } = null!;

    public Geometry Locatie { get; set; } = null!;

    public virtual ICollection<Medewerker> Medewerkers { get; set; } = new List<Medewerker>();

    public virtual ICollection<Prognose> Prognoses { get; set; } = new List<Prognose>();

    public virtual ICollection<Afdelingen> Afdelings { get; set; } = new List<Afdelingen>();

    public virtual ICollection<Openingstijden> Openingstijdens { get; set; } = new List<Openingstijden>();

}

