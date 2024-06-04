using Bumbo.Models;

namespace Bumbo.ViewModels
{
    public class InklokkenViewModel
    {
        public bool IsKlokt { get; set; }
        public DateTime ClockedIn { get; set; }
        public DateTime DateTime { get; set; } = DateTime.Now; //dit is geplaats om de huidige datum op klokkenpagina te tonen
        public string TimerMessage { get;  set; }
        public string ErrorMessage { get;  set; }
        public DateTime ClockedOut { get; set; }
    }
}
