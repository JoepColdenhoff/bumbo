using Bumbo.Models;
using Bumbo.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Bumbo.Controllers
{
    [Authorize(Roles = "Manager")]
    public class RoosterManagerController : Controller
    {
        private readonly BumboContext _context;

        public RoosterManagerController(BumboContext context)
        {
            _context = context;
        }


        [HttpGet]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Index(int year)
        {
            int currentYear = DateTime.Now.Year;
            int previousYear = year - 1;
            bool previewYearData = false;

            Medewerker loggedInUser = await GetLoggedInUser();
            int? filiaalId = loggedInUser?.FiliaalId;

            int currentWeek = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(
                DateTime.Now,
                CalendarWeekRule.FirstDay,
                DayOfWeek.Monday
            ) - 1;

            List<WeekGroup> checkDienstenCoverage = await CheckDienstenCoverage(year, filiaalId); // Assuming this is an async method

            List<WeekGroup> allWeeks = Enumerable.Range(1, 52).Select(weekNumber => new WeekGroup
            {
                WeekNumber = weekNumber,
                Amount = 0,
                isComplete = checkDienstenCoverage.Any(cdc => cdc.WeekNumber == weekNumber && cdc.isComplete)
            }).ToList();

            List<Diensten> roosterDataRaw = await _context.Dienstens
                .Where(d => d.Datum.Year == year && d.Medewerker.FiliaalId == filiaalId)
                .ToListAsync();

            if (!roosterDataRaw.Any() && year < currentYear)
            {
                TempData["TempData"] = $"Er bestaan geen roosters uit het jaar <b>{year}</b>";
                return RedirectToAction("Index", new { year = currentYear });
            }

            List<Diensten> previousRoosterDataRaw = await _context.Dienstens
                .Where(d => d.Datum.Year == previousYear && d.Medewerker.FiliaalId == filiaalId)
                .ToListAsync();

            if (previousRoosterDataRaw.Any())
            {
                previewYearData = true;
            }

            List<WeekGroup> roosterData = roosterDataRaw
                .GroupBy(dm => CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dm.Datum, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday))
                .Select(g => new WeekGroup
                {
                    WeekNumber = g.Key,
                    Amount = g.Count(),
                })
                .ToList();

            List<WeekGroup> dataMerge = allWeeks
                .GroupJoin(roosterData,
                           allWeek => allWeek.WeekNumber,
                           roosterWeek => roosterWeek.WeekNumber,
                           (allWeek, roosterWeeks) => new WeekGroup
                           {
                               WeekNumber = allWeek.WeekNumber,
                               Amount = roosterWeeks.FirstOrDefault()?.Amount ?? 0,
                               isComplete = allWeek.isComplete
                           })
                .OrderBy(w => w.WeekNumber)
                .ToList();



            var WeekViewModel = new RoosterJaarViewModel
            {
                Year = year,
                CurrentWeek = currentWeek,
                MergedData = dataMerge,
                PreviousRoosterYear = previewYearData
            };

            return View(WeekViewModel);
        }


        private async Task<List<WeekGroup>> CheckDienstenCoverage(int year, int? filiaalId)
        {
            CultureInfo cultureInfo = new CultureInfo("nl-NL");
            Calendar calendar = cultureInfo.Calendar;

            // Get all Prognoses for the given year and filiaal
            List<Prognose> prognoses = await _context.Prognoses
                .Include(p => p.Afdeling)
                .Where(p => p.Datum.Year == year && p.FiliaalId == filiaalId)
                .ToListAsync();

            // Get all Diensten for the given year and filiaal, including the Medewerker and their Afdeling
            List<Diensten> diensten = await _context.Dienstens
                .Include(d => d.Medewerker)
                    .ThenInclude(m => m.Functie)
                        .ThenInclude(f => f.Afdelings)
                .Where(d => d.Datum.Year == year && d.Medewerker.FiliaalId == filiaalId)
                .ToListAsync();

            List<WeekGroup> weeksCoverage = new List<WeekGroup>();

            // Get the distinct weeks in the prognoses
            IEnumerable<int> weeksInYear = prognoses.Select(p => calendar.GetWeekOfYear(p.Datum, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday)).Distinct();

            foreach (var week in weeksInYear)
            {
                IEnumerable<Prognose> prognosesInWeek = prognoses.Where(p => calendar.GetWeekOfYear(p.Datum, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday) == week);
                IEnumerable<Diensten> dienstenInWeek = diensten.Where(d => calendar.GetWeekOfYear(d.Datum, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday) == week);

                // Group by AfdelingId to compare the hours
                bool isWeekComplete = prognosesInWeek.GroupBy(p => p.AfdelingId).All(group =>
                {
                    var afdelingId = group.Key;
                    var prognoseHours = group.Sum(p => p.Uren);
                    var dienstHours = dienstenInWeek
                        .Where(d => d.Medewerker.Functie.Afdelings.Any(a => a.AfdelingId == afdelingId))
                        .Sum(d => (d.EindTijd - d.StartTijd).TotalHours);

                    return dienstHours >= prognoseHours;
                });

                weeksCoverage.Add(new WeekGroup
                {
                    WeekNumber = week,
                    isComplete = isWeekComplete
                });
            }

            return weeksCoverage;
        }


        private async Task<Medewerker> GetLoggedInUser()
        {
            var userEmail = User.Identity.Name;

            var medewerker = await _context.Medewerkers.FirstOrDefaultAsync(m => m.Email == userEmail);

            return medewerker;
        }


    }
}
