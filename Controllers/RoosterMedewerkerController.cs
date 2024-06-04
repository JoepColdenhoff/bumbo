using Bumbo.Models;
using Bumbo.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System.Globalization;

namespace Bumbo.Controllers
{
    [Authorize(Roles = "Medewerker")]
    public class RoosterMedewerkerController : Controller
    {
        private BumboContext _context;


        public RoosterMedewerkerController(BumboContext context)
        {
            _context = context;
        }


        public IActionResult Index()
        {
            int? tempId = GetLoggedInUser()?.MedewerkerId;

            var medewerker = _context.Medewerkers.FirstOrDefault(m => m.MedewerkerId == tempId);
            var vm = new MedewerkerDiensten();

            if (medewerker != null)
            {
                int functieId = medewerker.FunctieId ?? 0;

                DateTime currentDate = DateTime.Now.Date;

                vm.Diensten = _context.Dienstens
                    .Where(d => d.MedewerkerId == tempId && d.Datum >= currentDate)
                    .Select(d => new DienstViewModel
                    {
                        Dienst = d,
                        FunctieNaam = _context.Functies
                            .Where(f => f.FunctieId == functieId)
                            .Select(f => f.Naam)
                            .FirstOrDefault()
                    })
                    .OrderBy(d => d.Dienst.Datum)
                    .ToList();
            }

            return View(vm);
        }



        public IActionResult BeschikbaarheidCreate(int year, int weekNumber)
        {
            int? medewerkerId = GetLoggedInUser()?.MedewerkerId;

            DateTime firstDayOfWeek = FirstDateOfWeekISO8601(year, weekNumber);

            var beschikbaarheidList = Enumerable.Range(0, 7).SelectMany(day =>
                Enumerable.Range(0, 2).Select(slot => new BeschikbaarheidViewModel
                {
                    Datum = firstDayOfWeek.AddDays(day)
                }))
                .ToList();

            int currentYear = DateTime.Now.Year;
            var beschikbaarheidOptions = _context.Beschikbaarheids
                                       .Where(m => m.MedewerkerId == medewerkerId && m.Datum.Year == currentYear)
                                       .AsEnumerable()
                                       .GroupBy(p => GetIso8601WeekOfYear(p.Datum))
                                       .OrderBy(p => p.Key)
                                       .Select(g => g.Key)
                                       .ToList();


            if (TempData["Beschikbaarheid"] != null)
            {
                beschikbaarheidList = JsonConvert.DeserializeObject<List<BeschikbaarheidViewModel>>(TempData["Beschikbaarheid"].ToString());
                if (beschikbaarheidList.Count < 14) beschikbaarheidList = BeschikbaarheidsCombinedList(beschikbaarheidList, firstDayOfWeek);
            }

            var timeOptions = new List<TimeOnly> { TimeOnly.MinValue }; // Start with 00:00

            for (int hour = 7; hour <= 22; hour++)
            {
                timeOptions.Add(new TimeOnly(hour, 0)); // Add hour:00
                if (hour != 22) // Don't add 22:30
                {
                    timeOptions.Add(new TimeOnly(hour, 30)); // Add hour:30
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
        public IActionResult BeschikbaarheidCreate(BeschikbaarheidWeekViewModel beschikbaarheidViewModel)
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
                        return RedirectToAction("BeschikbaarheidCreate", new { beschikbaarheidViewModel, beschikbaarheidViewModel.Year, beschikbaarheidViewModel.WeekNumber });
                    }
                }
                previous = current;
            }

            Query(beschikbaarheidViewModel);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult BeschikbaarheidChange(int year, int weekNumber)
        {
            int? medewerkerId = GetLoggedInUser()?.MedewerkerId;

            var beschikbaarheid = _context.Beschikbaarheids
                                    .Where(m => m.MedewerkerId == medewerkerId && m.Datum.Year == year)
                                    .ToList()
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
        public IActionResult BeschikbaarheidCopy(BeschikbaarheidWeekViewModel beschikbaarheidWeek)
        {
            var copy = _context.Beschikbaarheids
                                .Where(m => m.MedewerkerId == beschikbaarheidWeek.MedewerkerId && m.Datum.Year == beschikbaarheidWeek.Year)
                                .ToList()
                                .Where(m => GetIso8601WeekOfYear(m.Datum) == beschikbaarheidWeek.CopyWeekNumber)
                                .Select(bm => new BeschikbaarheidViewModel
                                {
                                    //BeschikbaarheidId = bm.BeschikbaarheidId,
                                    MedewerkerId = bm.MedewerkerId,
                                    Datum = bm.Datum,
                                    SchoolUren = bm.SchoolUren,
                                    StartTijd = TimeOnly.FromTimeSpan(bm.StartTijd),
                                    EindTijd = TimeOnly.FromTimeSpan(bm.EindTijd),
                                })
                                .ToList();

            DateTime firstDayOfWeekCopy = FirstDateOfWeekISO8601(beschikbaarheidWeek.Year, beschikbaarheidWeek.CopyWeekNumber);
            DateTime firstDayOfWeekUpdate = FirstDateOfWeekISO8601(beschikbaarheidWeek.Year, beschikbaarheidWeek.WeekNumber);

            copy = BeschikbaarheidsCombinedList(copy, firstDayOfWeekCopy);
            beschikbaarheidWeek.BeschikbaarheidList = BeschikbaarheidsCombinedList(beschikbaarheidWeek.BeschikbaarheidList, firstDayOfWeekUpdate);

            for (int i = 0; i < copy.Count; i++)
            {
                beschikbaarheidWeek.BeschikbaarheidList[i].StartTijd = copy[i].StartTijd;
                beschikbaarheidWeek.BeschikbaarheidList[i].EindTijd = copy[i].EindTijd;
            }

            TempData["Beschikbaarheid"] = JsonConvert.SerializeObject(beschikbaarheidWeek.BeschikbaarheidList);

            return RedirectToAction("BeschikbaarheidCreate", new { beschikbaarheidWeek.Year, beschikbaarheidWeek.WeekNumber });
        }

        private void Query(BeschikbaarheidWeekViewModel beschikbaarWeekheidWeek)
        {
            var listWithId = beschikbaarWeekheidWeek.BeschikbaarheidList.Where(row => row.BeschikbaarheidId.HasValue && row.StartTijd != new TimeOnly(0, 0) && row.EindTijd != new TimeOnly(0, 0)).ToList();
            var listWithoutId = beschikbaarWeekheidWeek.BeschikbaarheidList.Where(row => !row.BeschikbaarheidId.HasValue && row.StartTijd != new TimeOnly(0, 0) && row.EindTijd != new TimeOnly(0, 0)).ToList();
            var listToDelete = beschikbaarWeekheidWeek.BeschikbaarheidList.Where(row => row.BeschikbaarheidId.HasValue && row.StartTijd == new TimeOnly(0, 0) && row.EindTijd == new TimeOnly(0, 0)).ToList();

            if (listToDelete.Any())
            {
                foreach (var itemDelete in listToDelete) 
                {
                    var deleteBeschikbaarheid = _context.Beschikbaarheids
                                                .Where(b => b.BeschikbaarheidId == itemDelete.BeschikbaarheidId)
                                                .FirstOrDefault();

                    var dienstCheck = _context.Dienstens
                                        .Where(b => b.BeschikbaarheidId == itemDelete.BeschikbaarheidId)
                                        .FirstOrDefault();
                    
                    if(dienstCheck == null)
                    {
                        _context.Remove(deleteBeschikbaarheid);
                    } else
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

            if(listWithoutId.Any())
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
            _context.SaveChanges();
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

    }
}



