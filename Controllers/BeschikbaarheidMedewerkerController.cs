using Bumbo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Bumbo.ViewModels;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;

namespace Bumbo.Controllers
{
    [Authorize(Roles = "Medewerker")]
    public class BeschikbaarheidMedewerkerController : Controller
    {
        private readonly BumboContext _context;

        public BeschikbaarheidMedewerkerController(BumboContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int year, int weekNumber)
        {
            bool isDienst = false;
            bool isUsed = false;

            int? medewerkerId = GetLoggedInUser()?.MedewerkerId;

            if (year == 0)
            {
                year = DateTime.Now.Year;
                weekNumber = GetIso8601WeekOfYear(DateTime.Now);
            }

            if(weekNumber >= 53)
            {
                year++;
            } else if (weekNumber <= 0)
            {
                year--;
            }

            DateTime firstDayOfWeek = FirstDateOfWeekISO8601(year, weekNumber);

            var weekDates = Enumerable.Range(0, 7).Select(i => firstDayOfWeek.AddDays(i)).ToList();

            var weekSchedule = new Dictionary<DateTime, List<Beschikbaarheid>>();

            foreach (var date in weekDates)
            {
                var beschikbaarheidsData = await _context.Beschikbaarheids
                    .Where(b => b.MedewerkerId == medewerkerId && b.Datum.Date == date)
                    .ToListAsync();
                if(beschikbaarheidsData != null)
                {
                    isUsed = true;

                    foreach (var beschikbaarheid in beschikbaarheidsData)
                    {
                        var dienst = await _context.Dienstens
                            .Where(d => d.MedewerkerId == medewerkerId && d.BeschikbaarheidId == beschikbaarheid.BeschikbaarheidId)
                            .FirstOrDefaultAsync();
                        if (dienst != null)
                        {
                            isDienst = true;
                        }
                    }
                }


                // Zorg ervoor dat er altijd twee beschikbaarheidsobjecten zijn voor elke dag.
                while (beschikbaarheidsData.Count < 2)
                {
                    beschikbaarheidsData.Add(new Beschikbaarheid { Datum = date, StartTijd = TimeSpan.Zero, EindTijd = TimeSpan.Zero });
                }

                weekSchedule[date] = beschikbaarheidsData;
            }

            weekSchedule = weekSchedule.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            var beschikbaarheidOverviewViewModel = new BeschikbaarheidOverviewViewModel
            {
                Year = year,
                WeekNumber = weekNumber,
                MedewerkerId = (int)medewerkerId,
                IsDienst = isDienst,
                IsUsed = isUsed,
                BeschikbaarheidList = weekSchedule
            };

            return View(beschikbaarheidOverviewViewModel);
        }


        [HttpGet]
        public async Task<IActionResult> BeschikbaarheidCreate(int year, int weekNumber)
        {
            int currentYear = DateTime.Now.Year;

            int? medewerkerId = GetLoggedInUser()?.MedewerkerId;

            DateTime firstDayOfWeek = FirstDateOfWeekISO8601(year, weekNumber);

            var beschikbaarheidList = Enumerable.Range(0, 7).SelectMany(day =>
                Enumerable.Range(0, 2).Select(slot => new BeschikbaarheidViewModel
                {
                    Datum = firstDayOfWeek.AddDays(day)
                }))
                .ToList();


            var beschikbaarheidData = await _context.Beschikbaarheids
                                    .Where(m => m.MedewerkerId == medewerkerId && m.Datum.Year == currentYear)
                                    .ToListAsync();

            var beschikbaarheidOptions = beschikbaarheidData
                .Where(p => GetIso8601WeekOfYear(p.Datum) != weekNumber)
                .GroupBy(p => GetIso8601WeekOfYear(p.Datum))
                .OrderBy(p => p.Key)
                .Select(g => g.Key)
                .Take(10)
                .ToList();


            if (TempData["Beschikbaarheid"] != null)
            {
                beschikbaarheidList = JsonConvert.DeserializeObject<List<BeschikbaarheidViewModel>>(TempData["Beschikbaarheid"].ToString());
                if (beschikbaarheidList.Count < 14) beschikbaarheidList = BeschikbaarheidsCombinedList(beschikbaarheidList, firstDayOfWeek);
            }

            var timeOptions = new List<TimeOnly> { TimeOnly.MinValue };

            for (int hour = 7; hour <= 22; hour++)
            {
                timeOptions.Add(new TimeOnly(hour, 0));
                if (hour != 22)
                {
                    timeOptions.Add(new TimeOnly(hour, 30));
                }
            }

            var beschikbaarheidWeekViewModel = new BeschikbaarheidWeekViewModel
            {
                Year = year,
                WeekNumber = weekNumber,
                MedewerkerId = (int)medewerkerId,
                TimeOptions = timeOptions,
                BeschikbaarheidOptions = beschikbaarheidOptions,
                BeschikbaarheidList = beschikbaarheidList
            };

            return View(beschikbaarheidWeekViewModel);
        }


        [HttpPost]
        public async Task<IActionResult> BeschikbaarheidCreate(BeschikbaarheidWeekViewModel beschikbaarheidViewModel)
        {
            BeschikbaarheidViewModel previous = null;

            foreach (var current in beschikbaarheidViewModel.BeschikbaarheidList)
            {
                if (!(current.StartTijd == new TimeOnly(0, 0) && current.EindTijd == new TimeOnly(0, 0)))
                {
                    // Check if StartTijd is earlier than EindTijd
                    if (current.StartTijd >= current.EindTijd)
                    {
                        TempData["IncorrectInput"] = $"<b>Eindtijd in {current.Datum.ToString("dddd d-M", CultureInfo.CurrentCulture)} is verkeerd ingevoerd.</b>";
                        TempData["Beschikbaarheid"] = JsonConvert.SerializeObject(beschikbaarheidViewModel.BeschikbaarheidList);
                        return RedirectToAction("BeschikbaarheidCreate", new { beschikbaarheidViewModel.Year, beschikbaarheidViewModel.WeekNumber });
                    }

                    // Check for overlapping times with the previous slot, but only if they are on the same day
                    if (previous != null && current.Datum == previous.Datum && current.StartTijd < previous.EindTijd)
                    {
                        TempData["IncorrectInput"] = $"<b>Tijden overlappen in {current.Datum.ToString("dddd d-M", CultureInfo.CurrentCulture)}.</b>";
                        TempData["Beschikbaarheid"] = JsonConvert.SerializeObject(beschikbaarheidViewModel.BeschikbaarheidList);
                        return RedirectToAction("BeschikbaarheidCreate", new { beschikbaarheidViewModel.Year, beschikbaarheidViewModel.WeekNumber });
                    }
                }
                previous = current;
            }

            await ProcessWeekAvailability(beschikbaarheidViewModel);

            TempData["Message"] = $"De beschikbaarheid voor week {beschikbaarheidViewModel.WeekNumber} is aangepast";

            return RedirectToAction("Index", new {beschikbaarheidViewModel.Year, beschikbaarheidViewModel.WeekNumber});
        }

        [HttpGet]
        public async Task<IActionResult> BeschikbaarheidChange(int year, int weekNumber)
        {
            int? medewerkerId = GetLoggedInUser()?.MedewerkerId;


            var beschikbaarheidData = await _context.Beschikbaarheids
                                        .Where(m => m.MedewerkerId == medewerkerId && m.Datum.Year == year)
                                        .ToListAsync();

            var beschikbaarheid = beschikbaarheidData
                                    .Where(m => GetIso8601WeekOfYear(m.Datum) == weekNumber)
                                    .Select(bm => new BeschikbaarheidViewModel
                                    {
                                    BeschikbaarheidId = bm.BeschikbaarheidId,
                                        MedewerkerId = bm.MedewerkerId,
                                        Datum = bm.Datum,
                                        SchoolUren = bm.SchoolUren,
                                        StartTijd = TimeOnly.FromTimeSpan(bm.StartTijd),
                                        EindTijd = TimeOnly.FromTimeSpan(bm.EindTijd),
                                    })
                                    .ToList();

            TempData["Beschikbaarheid"] = JsonConvert.SerializeObject(beschikbaarheid);
            return RedirectToAction("BeschikbaarheidCreate", new { year, weekNumber });
        }


        [HttpPost]
        public async Task<IActionResult> BeschikbaarheidCopy(BeschikbaarheidWeekViewModel beschikbaarheidWeek)
        {

            var beschikbaarheidData = await _context.Beschikbaarheids
                    .Where(m => m.MedewerkerId == beschikbaarheidWeek.MedewerkerId && m.Datum.Year == beschikbaarheidWeek.Year)
                    .ToListAsync();

            var weekNumbers = beschikbaarheidData   
                    .Where(m => GetIso8601WeekOfYear(m.Datum) == beschikbaarheidWeek.CopyWeekNumber)
                    .Select(bm => new BeschikbaarheidViewModel
                    {
                        MedewerkerId = bm.MedewerkerId,
                        Datum = bm.Datum,
                        SchoolUren = bm.SchoolUren,
                        StartTijd = TimeOnly.FromTimeSpan(bm.StartTijd),
                        EindTijd = TimeOnly.FromTimeSpan(bm.EindTijd),
                    })
                    .ToList();

            DateTime firstDayOfWeekCopy = FirstDateOfWeekISO8601(beschikbaarheidWeek.Year, beschikbaarheidWeek.CopyWeekNumber);
            DateTime firstDayOfWeekUpdate = FirstDateOfWeekISO8601(beschikbaarheidWeek.Year, beschikbaarheidWeek.WeekNumber);

            weekNumbers = BeschikbaarheidsCombinedList(weekNumbers, firstDayOfWeekCopy);
            beschikbaarheidWeek.BeschikbaarheidList = BeschikbaarheidsCombinedList(beschikbaarheidWeek.BeschikbaarheidList, firstDayOfWeekUpdate);

            for (int i = 0; i < weekNumbers.Count; i++)
            {
                beschikbaarheidWeek.BeschikbaarheidList[i].StartTijd = weekNumbers[i].StartTijd;
                beschikbaarheidWeek.BeschikbaarheidList[i].EindTijd = weekNumbers[i].EindTijd;
                beschikbaarheidWeek.BeschikbaarheidList[i].SchoolUren = weekNumbers[i].SchoolUren;
            }

            TempData["Beschikbaarheid"] = JsonConvert.SerializeObject(beschikbaarheidWeek.BeschikbaarheidList);

            return RedirectToAction("BeschikbaarheidCreate", new { beschikbaarheidWeek.Year, beschikbaarheidWeek.WeekNumber });
        }

        private async Task ProcessWeekAvailability(BeschikbaarheidWeekViewModel beschikbaarWeekheidWeek)
        {
            var listWithId = beschikbaarWeekheidWeek.BeschikbaarheidList.Where(row => row.BeschikbaarheidId.HasValue && row.StartTijd != new TimeOnly(0, 0) && row.EindTijd != new TimeOnly(0, 0)).ToList();
            var listWithoutId = beschikbaarWeekheidWeek.BeschikbaarheidList.Where(row => !row.BeschikbaarheidId.HasValue && row.StartTijd != new TimeOnly(0, 0) && row.EindTijd != new TimeOnly(0, 0)).ToList();
            var listToDelete = beschikbaarWeekheidWeek.BeschikbaarheidList.Where(row => row.BeschikbaarheidId.HasValue && row.StartTijd == new TimeOnly(0, 0) && row.EindTijd == new TimeOnly(0, 0)).ToList();

            if (listToDelete.Any())
            {
                foreach (var itemDelete in listToDelete)
                {
                    var deleteBeschikbaarheid = await _context.Beschikbaarheids
                                                    .Where(b => b.BeschikbaarheidId == itemDelete.BeschikbaarheidId)
                                                    .FirstOrDefaultAsync();

                    var dienstCheck = await _context.Dienstens
                                            .Where(b => b.BeschikbaarheidId == itemDelete.BeschikbaarheidId)
                                            .FirstOrDefaultAsync();


                    if (dienstCheck == null)
                    {
                        _context.Remove(deleteBeschikbaarheid);
                    }
                    else
                    {
                        TempData["IncorrectInput"] = $"<b>U bent al ingeroosterd op {itemDelete.Datum.ToString("dddd d-M", CultureInfo.CurrentCulture)}. U kunt die tijd niet veranderen.</b>";
                        TempData["Beschikbaarheid"] = JsonConvert.SerializeObject(beschikbaarWeekheidWeek.BeschikbaarheidList);
                        return;
                    }
                }
            }

            if (listWithId.Any())
            {
                foreach (var itemUpdate in listWithId)
                {
                    var beschikbaarheid = new Beschikbaarheid
                    {
                        BeschikbaarheidId = (int)itemUpdate.BeschikbaarheidId,
                        MedewerkerId = beschikbaarWeekheidWeek.MedewerkerId,
                        Datum = itemUpdate.Datum,
                        SchoolUren = itemUpdate.SchoolUren,
                        StartTijd = itemUpdate.StartTijd.ToTimeSpan(),
                        EindTijd = itemUpdate.EindTijd.ToTimeSpan(),
                    };
                    _context.Update(beschikbaarheid);
                }
            }

            if (listWithoutId.Any())
            {
                foreach (var itemInsert in listWithoutId)
                {
                    var beschikbaarheid = new Beschikbaarheid
                    {
                        MedewerkerId = beschikbaarWeekheidWeek.MedewerkerId,
                        Datum = itemInsert.Datum,
                        SchoolUren = itemInsert.SchoolUren,
                        StartTijd = itemInsert.StartTijd.ToTimeSpan(),
                        EindTijd = itemInsert.EindTijd.ToTimeSpan(),
                    };
                    _context.Add(beschikbaarheid);
                }
            }
            await _context.SaveChangesAsync();
        }


        private List<BeschikbaarheidViewModel> BeschikbaarheidsCombinedList(List<BeschikbaarheidViewModel> beschikbaarheidMedewerker, DateTime firstDayOfWeek)
        {
            var fullWeekDates = Enumerable.Range(0, 7).Select(offset => firstDayOfWeek.AddDays(offset)).ToList();

            // Combine actual entries and placeholders, ensuring each date has up to two entries.
            var combinedList = fullWeekDates
                .SelectMany(date =>
                {
                    var entriesForDate = beschikbaarheidMedewerker.Where(b => b.Datum.Date == date).ToList();
                    var countToAdd = 2 - entriesForDate.Count; // Determine how many placeholders to add (0, 1, or 2).

                    // Add placeholders if necessary.
                    for (int i = 0; i < countToAdd; i++)
                    {
                        entriesForDate.Add(new BeschikbaarheidViewModel
                        {
                            Datum = date,
                            SchoolUren = null,
                            StartTijd = TimeOnly.MinValue,
                            EindTijd = TimeOnly.MinValue,
                        });
                    }

                    return entriesForDate;
                })
                .OrderBy(b => b.Datum)
                .ThenByDescending(b => b.SchoolUren.HasValue && b.SchoolUren > 0) // Non-null and non-zero SchoolUren come first.
                .ThenByDescending(b => b.StartTijd != TimeOnly.MinValue) // Non-default StartTijd comes first.
                .ThenBy(b => b.StartTijd) // Then sort by StartTijd.
                .ToList();

            return combinedList;
        }


        private static DateTime FirstDateOfWeekISO8601(int year, int weekNumber)
        {
            // Calculate the date of the first Thursday of the year
            DateTime firstDayOfYear = new DateTime(year, 1, 1);
            int daysUntilThursday = (DayOfWeek.Thursday - firstDayOfYear.DayOfWeek + 7) % 7;
            DateTime firstThursday = firstDayOfYear.AddDays(daysUntilThursday);
            // Calculate the start of the first week
            DateTime startOfFirstWeek = firstThursday.AddDays(-3);
            DateTime firstDayOfWeek = startOfFirstWeek.AddDays((weekNumber - 1) * 7);

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

        private Medewerker GetLoggedInUser()
        {
            var userEmail = User.Identity.Name;

            var medewerker = _context.Medewerkers.FirstOrDefault(m => m.Email == userEmail);

            return medewerker;
        }

        // Functie om de eerste dag van de week als maandag te bepalen
        private DateTime GetFirstDayOfWeekMonday(DateTime date)
        {
            // Bepaal de eerste dag van de week als maandag.
            int delta = DayOfWeek.Monday - date.DayOfWeek;
            if (delta > 0) delta -= 7;
            return date.AddDays(delta);
        }
    }
}

