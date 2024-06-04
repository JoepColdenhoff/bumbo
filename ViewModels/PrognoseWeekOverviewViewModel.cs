namespace Bumbo.ViewModels;

public class PrognoseWeekOverviewViewModel
{
    public List<DateTime> Dagen { get; set; } = new List<DateTime>();

    public List<string> Afdelingen { get; set; } = new List<string>();

    public List<List<int>> Uren { get; set; } = new List<List<int>>(); // Nested list to store the "uren" for each "afdeling" on each day

    public int Year { get; set; }
}
