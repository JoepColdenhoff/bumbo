using Bumbo.Models;

namespace Bumbo.ViewModels;

public class PrognoseJaarViewModel
{
    public int Year { get; set; }

    public int AmountAfdelingen { get; set; }

    public int CurrentWeek { get; set; }

    public List<WeekGroup> MergedData { get; set; }

    public bool PreviousPrognoseYear { get; set; }
}
