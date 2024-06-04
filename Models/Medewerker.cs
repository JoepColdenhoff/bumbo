using System;
using System.Collections.Generic;

namespace Bumbo.Models;

public partial class Medewerker
{
    public int MedewerkerId { get; set; }

    public int? FunctieId { get; set; }

    public int? FiliaalId { get; set; }

    public string Voornaam { get; set; } = null!;

    public string? Tussenvoegsel { get; set; }

    public string Achternaam { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Telefoonnummer { get; set; } = null!;

    public DateTime Geboortedatum { get; set; }

    public string Postcode { get; set; } = null!;

    public string Huisnummer { get; set; }

    public string Straatnaam { get; set; } = null!;

    public string Plaats { get; set; } = null!;

    public DateTime Indienst { get; set; }

    public bool Verwijdert { get; set; }

    public virtual ICollection<Beschikbaarheid> Beschikbaarheids { get; set; } = new List<Beschikbaarheid>();

    public virtual ICollection<Diensten> Dienstens { get; set; } = new List<Diensten>();

    public virtual Filialen? Filiaal { get; set; }

    public virtual Functie? Functie { get; set; }
}
