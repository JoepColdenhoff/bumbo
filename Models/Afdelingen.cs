namespace Bumbo.Models;

public partial class Afdelingen
{
    public int AfdelingId { get; set; }

    public string Naam { get; set; } = null!;

    public int AfdelingGroteInMeters { get; set; }

    public virtual ICollection<Activiteiten> Activiteitens { get; set; } = new List<Activiteiten>();

    public virtual ICollection<Prognose> Prognoses { get; set; } = new List<Prognose>();

    public virtual ICollection<Filialen> Filiaals { get; set; } = new List<Filialen>();

    public virtual ICollection<Functie> Functies { get; set; } = new List<Functie>();
}
