using System;
using System.Collections.Generic;

namespace Bumbo.Models;

public partial class Normeringen
{
    public int NormeringId { get; set; }

    public string Eenheid { get; set; } = null!;

    public DateTime UploadDatum { get; set; }

    public int Duur { get; set; }

    public virtual ICollection<Activiteiten> Activiteitens { get; set; } = new List<Activiteiten>();
}
