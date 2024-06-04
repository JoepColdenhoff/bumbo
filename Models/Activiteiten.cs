using System;
using System.Collections.Generic;

namespace Bumbo.Models;

public partial class Activiteiten
{
    public int ActiviteitenId { get; set; }

    public int? AfdelingId { get; set; }

    public string Naam { get; set; } = null!;

    public string? Beschrijving { get; set; }

    public virtual Afdelingen? Afdeling { get; set; }

    public virtual ICollection<Normeringen> Normerings { get; set; } = new List<Normeringen>();
}
