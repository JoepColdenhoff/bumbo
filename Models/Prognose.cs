using System;
using System.Collections.Generic;

namespace Bumbo.Models;

public partial class Prognose
{
    public int PrognoseId { get; set; }

    public int AfdelingId { get; set; }

    public int FiliaalId { get; set; }

    public DateTime Datum { get; set; }

    public int PotentieleAantalBezoekers { get; set; }

    public int AantalCollies { get; set; }

    public int Uren { get; set; }

    public bool Vakantiedag { get; set; }

    public virtual Afdelingen Afdeling { get; set; } = null!;

    public virtual Filialen Filiaal { get; set; } = null!;
}
