using System;
using System.Collections.Generic;

namespace Bumbo.Models;

public partial class Beschikbaarheid
{
    public int BeschikbaarheidId { get; set; }

    public int? MedewerkerId { get; set; }

    public DateTime Datum { get; set; }

    public int? SchoolUren { get; set; }

    public TimeSpan EindTijd { get; set; }

    public TimeSpan StartTijd { get; set; }

    public virtual Diensten? Diensten { get; set; }

    public virtual Medewerker? Medewerker { get; set; }
}

