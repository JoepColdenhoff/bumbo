namespace Bumbo.Models
{
    public partial class Openingstijden
    {
        public int OpeningstijdenId { get; set; }

        public int DagVanWeek { get; set; }

        public TimeSpan OpeningsTijd { get; set; }

        public TimeSpan SluitingsTijd { get; set; }

        public virtual ICollection<Filialen> Filialens { get; set; } = new List<Filialen>();
    }
}
