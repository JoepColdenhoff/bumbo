using System.Globalization;
using Bumbo.Models;

namespace Bumbo.ViewModels
{
    public class PrognoseFormViewModel
    {
        public List<Prognose> PrognosesList { get; set; }

        public List<bool> isVacation { get; set; }

        public DateTime Date { get; set; }

        public CultureInfo Culture { get; set; }

        public int WeekNumber { get; internal set; }

        public int Year { get; internal set; }
    }
}
