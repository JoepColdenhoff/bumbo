using Bumbo.Models;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Microsoft.CodeAnalysis;
using Bumbo.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Bumbo.Controllers
{
    [Authorize(Roles = "Manager")]
    public class PrognoseController : Controller
    {
        private readonly BumboContext _context;

        private int secondsInMinute = 60;

        public PrognoseController(BumboContext context, ILogger<PrognoseController> logger)
        {
            _context = context;
        }


        [HttpGet]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Index(int year)
        {
            int yearIndex = year;
            int currentYear = DateTime.Now.Year;
            int lastYear = yearIndex - 1;
            bool previewYearData = false;

            int? filiaalid = GetLoggedInUser().FiliaalId;

            var amountAfdelingen = await _context.Afdelingens
                                        .Where(a => a.Filiaals.Any(f => f.FiliaalId == filiaalid))
                                        .CountAsync();

            int currentWeek = CultureInfo.CurrentCulture.DateTimeFormat.Calendar.GetWeekOfYear(
                DateTime.Now,
                CalendarWeekRule.FirstDay,
                DayOfWeek.Monday
            ) - 1;

            List<WeekGroup> allWeeks = Enumerable.Range(1, 52).Select(weekNumber => new WeekGroup
            {
                WeekNumber = weekNumber,
                Amount = 0
            }).ToList();

            DateTime yearStart = new DateTime(year, 1, 1);
            DateTime yearEnd = new DateTime(year, 12, 31);


            List<Prognose> prognoseDataRaw = await _context.Prognoses
                .Where(a => a.FiliaalId == filiaalid && a.Datum >= yearStart && a.Datum <= yearEnd)
                .ToListAsync();

            if (!prognoseDataRaw.Any() && year < currentYear)
            {
                TempData["TempData"] = $"Er bestaan geen Prognoses uit het jaar <b>{year}</b>";
                return RedirectToAction("Index", new { Year = currentYear });
            }

            DateTime previousYearStart = new DateTime(lastYear, 1, 1);
            DateTime previousYearEnd = new DateTime(lastYear, 12, 31);

            List<Prognose> prognosePreviousYearDataRaw = await _context.Prognoses
                .Where(a => a.FiliaalId == filiaalid && a.Datum >= previousYearStart && a.Datum <= previousYearEnd)
                .Take(1)
                .ToListAsync();

            if (prognosePreviousYearDataRaw.Any())
            {
                previewYearData = true;
            }


            // Query that retrieves data from the database
            List<WeekGroup> prognoseData = prognoseDataRaw
                .GroupBy(p => GetIso8601WeekOfYear(p.Datum))
                .Select(g => new WeekGroup
                {
                    WeekNumber = g.Key,
                    Amount = g.Count(),
                })
                .OrderBy(wg => wg.WeekNumber)
                .ToList();

            // Merge database data with week numbers
            List<WeekGroup> mergedData = allWeeks
                        .GroupJoin(prognoseData,
                                   allWeek => allWeek.WeekNumber,
                                   actualWeek => actualWeek.WeekNumber,
                                   (allWeek, actualWeeks) => actualWeeks.Any()
                                       ? actualWeeks.Single()
                                       : allWeek)
                        .OrderBy(p => p.WeekNumber)
                        .ToList();


            var viewModel = new PrognoseJaarViewModel
            {
                Year = year,
                AmountAfdelingen = amountAfdelingen,
                CurrentWeek = currentWeek,
                MergedData = mergedData,
                PreviousPrognoseYear = previewYearData
            };

            return View(viewModel);
        }


        [HttpGet]
        public IActionResult PrognoseCreateForm(int weekNumber, int year)
        {
            var culture = new CultureInfo("nl-NL");

            var prognosesList = new List<Prognose>();

            for (int i = 0; i < 7; i++) //7 stands for 7 days
            {
                var prognose = new Prognose();

                prognosesList.Add(prognose);
            }

            var prognoseFormViewModel = new PrognoseFormViewModel
            {
                PrognosesList = prognosesList,
                Culture = culture,
                WeekNumber = weekNumber,
                Year = year
            };

            return View(prognoseFormViewModel);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult PrognoseCreateForm(PrognoseFormViewModel formViewModel, int weekNumber, int year)
        {
            int test = year;
            var startDate = FirstDateOfWeekISO8601(test, weekNumber);

            int filiaalId = (int)GetLoggedInUser()?.FiliaalId;

            var counter = 0;
            var afdelingen = _context.Afdelingens
                .Where(a => a.Filiaals.Any(f => f.FiliaalId == filiaalId))
                .ToList();

            int versKlanten = GetActivityDuration("Vers", filiaalId);
            int uitLadenColli = GetActivityDuration("Coli uitladen", filiaalId);
            int colliVakkenvullen = GetActivityDuration("Vakken vullen", filiaalId);
            int spiegelen = GetActivityDuration("Spiegelen", filiaalId);
            int kassaKlanten = GetActivityDuration("Kassa", filiaalId);
            int aantalMetersVers = GetDepartmentSizeInMeters("Vers", filiaalId);
            int aantalMetersVakkenvullers = GetDepartmentSizeInMeters("Vakkenvullers", filiaalId);


            foreach (var prognoseViewModel in formViewModel.PrognosesList)
            {
                foreach (var afdeling in afdelingen)
                {
                    var afdelingNaam = _context.Afdelingens
                        .Where(a => a.AfdelingId == afdeling.AfdelingId)
                        .Select(a => a.Naam)
                        .FirstOrDefault();

                    var prognose = new Prognose()
                    {
                        Datum = startDate.AddDays(counter),
                        FiliaalId = (int)filiaalId,
                        AfdelingId = afdeling.AfdelingId,
                        AantalCollies = prognoseViewModel.AantalCollies,
                        Vakantiedag = prognoseViewModel.Vakantiedag
                    };
                    if (prognoseViewModel.Vakantiedag)
                    {
                        prognose.PotentieleAantalBezoekers = (int)Math.Round(prognoseViewModel.PotentieleAantalBezoekers * 1.10);
                    }
                    else
                    {
                        prognose.PotentieleAantalBezoekers = prognoseViewModel.PotentieleAantalBezoekers;
                    }
                    if (afdelingNaam == "Vers")
                    {
                        prognose.Uren = prognose.PotentieleAantalBezoekers / versKlanten + (prognose.AantalCollies * uitLadenColli + prognose.AantalCollies * colliVakkenvullen + aantalMetersVers * (spiegelen / secondsInMinute)) / secondsInMinute;
                        //Aantal uur vers = Aantal klanten / 100 + (Aantal Collies * 5 + Aantal Collies * 30 + Aantal meters * 0,5) / 60
                    }
                    else if (afdelingNaam == "Vakkenvuller")
                    {
                        prognose.Uren = (prognose.AantalCollies * uitLadenColli + prognose.AantalCollies * colliVakkenvullen + aantalMetersVers * (spiegelen / secondsInMinute)) / secondsInMinute;
                        //Aantal uur vakkenvullers = (Aantal Collies * 5 + Aantal Collies * 30 + Aantal meters * 0,5) / 60

                    }
                    else if (afdelingNaam == "Kassa")
                    {
                        prognose.AantalCollies = 0;
                        prognose.Uren = prognose.PotentieleAantalBezoekers / kassaKlanten;
                        //Aantal uur kassa = (Aantal klanten / 30)
                    }

                    _context.Prognoses.Add(prognose);
                };
                counter++;
            }

            _context.SaveChanges();
            return RedirectToAction("PrognoseWeekOverview", new { WeekNumber = weekNumber, Year = year });
        }


        public async Task<IActionResult> PrognoseUpdateForm(int weekNumber, int year)
        {
            var culture = new CultureInfo("nl-NL");
            var startDate = FirstDateOfWeekISO8601(year, weekNumber);
            var endDate = startDate.AddDays(7);

            // Verwijder prognoses die vallen binnen de start- en einddatum
            var prognosesList = _context.Prognoses
                .Where(p => p.Datum >= startDate && p.Datum < endDate)
                .GroupBy(p => p.Datum)
                .Select(group => group.First())
                .ToList();

            foreach (Prognose prognose in prognosesList)
            {
               if (prognose.Vakantiedag)
                {
                    prognose.PotentieleAantalBezoekers = (int)Math.Round(prognose.PotentieleAantalBezoekers * 0.90);
                }
            }

            var prognoseFormViewModel = new PrognoseFormViewModel
            {
                PrognosesList = prognosesList,
                Culture = culture,
                WeekNumber = weekNumber,
                Year = year
            };

            return View(prognoseFormViewModel);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult PrognoseUpdateForm(PrognoseFormViewModel formViewModel, int weekNumber, int year)
        {
            int filiaalId = (int)GetLoggedInUser()?.FiliaalId;

            var startDate = FirstDateOfWeekISO8601(year, weekNumber);
            var endDate = startDate.AddDays(7);
            var prognosesList = _context.Prognoses
               .Where(p => p.Datum >= startDate && p.Datum < endDate)
               .ToList();
            var afdelingen = _context.Afdelingens
                .Where(a => a.Filiaals.Any(f => f.FiliaalId == filiaalId))
                .ToList();

            var prognoseCounter = 0;
            var vacationCounter = 0;

            int versKlanten = GetActivityDuration("Vers", filiaalId);
            int uitLadenColli = GetActivityDuration("Coli uitladen", filiaalId);
            int colliVakkenvullen = GetActivityDuration("Vakken vullen", filiaalId);
            int spiegelen = GetActivityDuration("Spiegelen", filiaalId);
            int kassaKlanten = GetActivityDuration("Kassa", filiaalId);
            int aantalMetersVers = GetDepartmentSizeInMeters("Vers", filiaalId);
            int aantalMetersVakkenvullers = GetDepartmentSizeInMeters("Vakkenvullers", filiaalId);

            foreach (var prognoseViewModel in formViewModel.PrognosesList)
            {
                foreach (var afdeling in afdelingen)
                {
                    var afdelingNaam = _context.Afdelingens
                       .Where(a => a.AfdelingId == afdeling.AfdelingId)
                       .Select(a => a.Naam)
                       .FirstOrDefault();

                    prognosesList[prognoseCounter].AantalCollies = prognoseViewModel.AantalCollies;
                    prognosesList[prognoseCounter].Vakantiedag = prognoseViewModel.Vakantiedag;

                    if (prognosesList[prognoseCounter].Vakantiedag)
                    {
                        prognosesList[prognoseCounter].PotentieleAantalBezoekers = (int)Math.Round(prognoseViewModel.PotentieleAantalBezoekers * 1.10);
                    }
                    else
                    {
                        prognosesList[prognoseCounter].PotentieleAantalBezoekers = prognoseViewModel.PotentieleAantalBezoekers;
                    }
                    
                    if (afdelingNaam == "Vers")
                    {
                        prognosesList[prognoseCounter].Uren = prognosesList[prognoseCounter].PotentieleAantalBezoekers / versKlanten + (prognosesList[prognoseCounter].AantalCollies * uitLadenColli + prognosesList[prognoseCounter].AantalCollies * colliVakkenvullen + aantalMetersVers * (spiegelen / secondsInMinute)) / secondsInMinute;
                        //Aantal uur vers = Aantal klanten / 100 + (Aantal Collies * 5 + Aantal Collies * 30 + Aantal meters * 0,5) / 60
                    }
                    else if (afdelingNaam == "Vakkenvullers")
                    {
                        prognosesList[prognoseCounter].Uren = (prognosesList[prognoseCounter].AantalCollies * uitLadenColli + prognosesList[prognoseCounter].AantalCollies * colliVakkenvullen + aantalMetersVers * (spiegelen / secondsInMinute)) / secondsInMinute;
                        //Aantal uur vakkenvullers = (Aantal Collies * 5 + Aantal Collies * 30 + Aantal meters * 0,5) / 60
                    }
                    else if (afdelingNaam == "Kassa")
                    {
                        prognosesList[prognoseCounter].AantalCollies = 0;
                        prognosesList[prognoseCounter].Uren = prognosesList[prognoseCounter].PotentieleAantalBezoekers / kassaKlanten;
                        //Aantal uur kassa = (Aantal klanten / 30)
                    }
                    _context.Update(prognosesList[prognoseCounter]);
                    prognoseCounter++;
                };
                vacationCounter++;
            }
            _context.SaveChanges();
            return RedirectToAction("PrognoseWeekOverview", new { weekNumber = weekNumber, year = year });
        }

        public IActionResult PrognoseWeekOverview(int weekNumber, int year)
        {
            var startDate = FirstDateOfWeekISO8601(year, weekNumber);
            var dagen = Enumerable.Range(0, 7).Select(offset => startDate.AddDays(offset)).ToList();

            var afdelingenNamen = new List<string> { "Vers", "Vakkenvullers", "Kassa" };
            var afdelingenIds = new List<int> { 1, 2, 3 };

            var urenPerDagPerAfdeling = dagen.Select(dag =>
            {
                return afdelingenIds.Select(id =>
                {
                    // Sum the hours for each department per day
                    var totalUren = _context.Prognoses
                                    .Where(p => p.Datum == dag && p.AfdelingId == id)
                                    .Sum(p => (int?)p.Uren) ?? 0; // Cast to decimal? to allow for null values, defaulting to 0 if none
                    return totalUren;
                }).ToList();
            }).ToList();

            var viewModel = new PrognoseWeekOverviewViewModel
            {
                Dagen = dagen,
                Afdelingen = afdelingenNamen,
                Uren = urenPerDagPerAfdeling,
                Year = year,
            };

            return View(viewModel);
        }


        [HttpPost]
        public IActionResult DeletePrognose(int weekNumber, int year)
        {
            try
            {
                var startDate = FirstDateOfWeekISO8601(DateTime.Now.Year, weekNumber);
                var endDate = startDate.AddDays(7);

                // Verwijder prognoses die vallen binnen de start- en einddatum
                var prognosesToRemove = _context.Prognoses
                    .Where(p => p.Datum >= startDate && p.Datum < endDate)
                    .ToList();

                _context.Prognoses.RemoveRange(prognosesToRemove);
                _context.SaveChanges();

                TempData["Message"] = "Prognose voor week " + weekNumber + " is succesvol verwijderd.";
            }
            catch (Exception ex)
            {
                // Log de exception als je logging hebt ingeschakeld 
                // LogError(ex);

                TempData["ErrorMessage"] = "Er is iets misgegaan bij het verwijderen van de prognose voor week " + weekNumber + ".";
            }

            return RedirectToAction("Index", new { year = year });
        }

        // This function gets the first day of the week for the given year and week number, following ISO 8601.
        private static DateTime FirstDateOfWeekISO8601(int year, int weekOfYear)
        {
            // Calculate the date of the first Thursday of the year
            DateTime firstDayOfYear = new DateTime(year, 1, 1);
            int daysUntilThursday = (DayOfWeek.Thursday - firstDayOfYear.DayOfWeek + 7) % 7;
            DateTime firstThursday = firstDayOfYear.AddDays(daysUntilThursday);
            // Calculate the start of the first week
            DateTime startOfFirstWeek = firstThursday.AddDays(-3);
            DateTime firstDayOfWeek = startOfFirstWeek.AddDays((weekOfYear - 1) * 7);

            return firstDayOfWeek;
        }

        private static int GetIso8601WeekOfYear(DateTime date)
        {
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(date);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                date = date.AddDays(3);
            }

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        private int GetActivityDuration(string activityName, int filiaalId)
        {
            return _context.Normeringens
                .SelectMany(n => n.Activiteitens)
                .Where(a => a.Naam == activityName && a.Afdeling.Filiaals.Any(f => f.FiliaalId == filiaalId))
                .Select(a => a.Normerings.FirstOrDefault().Duur)
                .FirstOrDefault();
        }

        private int GetDepartmentSizeInMeters(string departmentName, int filiaalId)
        {
            var departmentSize = _context.Afdelingens
                .Where(a => a.Naam == departmentName && a.Filiaals.Any(f => f.FiliaalId == filiaalId))
                .Select(a => a.AfdelingGroteInMeters)
                .FirstOrDefault();
            //.GetValueOrDefault();

            return departmentSize;
        }

        private Medewerker GetLoggedInUser()
        {
            var userEmail = User.Identity.Name;

            var medewerker = _context.Medewerkers.FirstOrDefault(m => m.Email == userEmail);

            return medewerker;
        }

    }


}
