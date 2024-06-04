using System;
using System.Collections.Generic;

namespace Bumbo.Models;

public partial class Diensten
{
    public int DienstenId { get; set; }

    public int? MedewerkerId { get; set; }

    public TimeSpan StartTijd { get; set; }

    public TimeSpan EindTijd { get; set; }

    public DateTime Datum { get; set; }

    public int AfdelingId { get; set; }

    public int? BeschikbaarheidId { get; set; }

    public bool Ziek { get; set; }

    public virtual Beschikbaarheid? Beschikbaarheid { get; set; }

    public virtual ICollection<Inklokken> Inklokken { get; set; } = new List<Inklokken>();

    public virtual Medewerker? Medewerker { get; set; }

    public virtual Afdelingen? Afdelingen { get; set; }
}
