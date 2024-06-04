using Bumbo.Models;
using System.Collections.Generic;

namespace Bumbo.ViewModels
{
    public class TestCAOCheckViewModel
    {
        public List<Medewerker> Medewerkers { get; set; }
        public List<Beschikbaarheid> Beschikbaarheden { get; set; } 
        public List<Beschikbaarheid> Ingepland {  get; set; }   
        public int SelectedMedewerkerId { get; set; }
        public int SelectedBeschikbaarheidId { get; set; } 
        public CAOErrorViewModel CAOCheckResult { get; set; }
        public string ErrorMessage { get; internal set; }

        public TestCAOCheckViewModel()
        {
            Ingepland = new List<Beschikbaarheid>();
        }
    }
}
