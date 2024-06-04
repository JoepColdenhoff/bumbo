/*using Bumbo.Controllers;
using Bumbo.ExceptionClasses;
using Bumbo.Models;


namespace CAOCheckTest;

[TestFixture]
public class Tests
{
    private BumboContext _context;
    private CAOController _caoController;

    [SetUp]
    public void Setup()
    {
        _context = new BumboContext();
        // Initialize CAOController with a mock context or necessary dependencies
        _caoController = new CAOController(_context);
    }

    [Test]
    public void TestCheckAgeShiftRulesUnder16_ExceedsSchoolHours()
    {
        // Arrange
        var medewerker = new Medewerker
        {
            Geboortedatum = DateTime.Now.AddYears(-15),
            Beschikbaarheids = new List<Beschikbaarheid>
            {
                new Beschikbaarheid
                {
                    EindTijd = TimeSpan.FromHours(10),
                    StartTijd = TimeSpan.FromHours(8),
                    SchoolUren = 4
                }
            }
        };

        // Act & Assert
        Assert.That(() => _caoController.CheckAgeShiftRules(medewerker), Throws.Nothing);
    }

    [Test]
    public void TestCheckAgeShiftRulesUnder16_ExceedsOtherRules()
    {
        var medewerker = new Medewerker
        {
            Geboortedatum = DateTime.Now.AddYears(-15),
            Beschikbaarheids = new List<Beschikbaarheid>
            {
                new Beschikbaarheid
                {
                    EindTijd = TimeSpan.FromHours(20), // werkt na  19:00
                    StartTijd = TimeSpan.FromHours(8),
                    SchoolUren = 5, // boven de 12 uren school per week
                }
            }
        };

        Assert.Throws<CAOException>(() => _caoController.CheckAgeShiftRules(medewerker));
    }

    [Test]
    public void TestCheckAgeShiftRulesAge16or17_ExceedsSchoolHours()
    {
        var medewerker = new Medewerker
        {
            Geboortedatum = DateTime.Now.AddYears(-16),
            Beschikbaarheids = new List<Beschikbaarheid>
            {
                new Beschikbaarheid
                {
                    EindTijd = TimeSpan.FromHours(10),
                    StartTijd = TimeSpan.FromHours(8),
                    SchoolUren = 4
                }
            }
        };

        // Act & Assert
        Assert.That(() => _caoController.CheckAgeShiftRules(medewerker), Throws.Nothing);
    }

    [Test]
    public void TestCheckAgeShiftRulesAge16or17_ExceedsOtherRules()
    {
        var medewerker = new Medewerker
        {
            Geboortedatum = DateTime.Now.AddYears(-16),
            Beschikbaarheids = new List<Beschikbaarheid>
            {
                new Beschikbaarheid
                {
                    EindTijd = TimeSpan.FromHours(22), // Werkt na  19:00
                    StartTijd = TimeSpan.FromHours(8),
                    SchoolUren = 5, // boven de 9 uur met school
                }
            }
        };

        // Act & Assert
        Assert.Throws<CAOException>(() => _caoController.CheckAgeShiftRules(medewerker));
    }

}
*/