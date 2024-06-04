using System;
using System.Collections.Generic;

namespace Bumbo.Models;

public partial class Functie
{
    public int FunctieId { get; set; }

    public string Naam { get; set; } = null!;

    public int Schaal { get; set; }

    public string? Beschrijving { get; set; }

    public virtual ICollection<Medewerker> Medewerkers { get; set; } = new List<Medewerker>();

    public virtual ICollection<Afdelingen> Afdelings { get; set; } = new List<Afdelingen>();
}
