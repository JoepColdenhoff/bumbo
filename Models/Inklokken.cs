namespace Bumbo.Models
{
    public partial class Inklokken
    {
        public int InklokkenId { get; set; }

        public int DienstenId { get; set; }

        public TimeSpan Start { get; set; }

        public TimeSpan? Eind { get; set; }

        public bool Goedkeuring { get; set; }

        public virtual Diensten? Diensten { get; set; }
    }
}
