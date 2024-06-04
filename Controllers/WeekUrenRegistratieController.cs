using Bumbo.Models;
using Bumbo.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared.CodeModifier.CodeChange;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Bumbo.Controllers
{
    [Authorize(Roles = "Manager")]
    public class WeekUrenRegistratieController : Controller
    {

        private readonly BumboContext _context;


        public WeekUrenRegistratieController(BumboContext context)
        {
            _context = context;
        }



        [HttpGet]
        public async Task<IActionResult> Index(int year)
        {

            int currentYear = DateTime.Now.Year;
            int previousYear = year - 1;
            int nextYear = year + 1;
            bool previewHourRegister = false;
            bool nextHourRegister = false;

            int? filiaalid = GetLoggedInUser()?.FiliaalId;

            int currentWeek = GetIso8601WeekOfYear(DateTime.Now);

            List<WeekGroup> allWeeks = Enumerable.Range(1, 52).Select(weekNumber => new WeekGroup
            {
                WeekNumber = weekNumber,
                isComplete = true // Initialize as complete
            }).ToList();


            List<Inklokken> hoursRegister = await _context.Inklokkens
                                    .Include(i => i.Diensten)
                                    .Where(i => i.Diensten.Datum.Year == year)
                                    .ToListAsync();

            if (!hoursRegister.Any() && year < currentYear || !hoursRegister.Any() && year > currentYear)
            {
                TempData["TempData"] = $"Er bestaan geen uren uit het jaar <b>{year}</b>";
                return RedirectToAction("Index", new { Year = currentYear });
            }

            List<Inklokken> previousYearHoursRegisterData = await _context.Inklokkens
                        .Include(i => i.Diensten)
                        .Where(i => i.Diensten.Datum.Year == previousYear)
                        .Take(1)
                        .ToListAsync();

            if (previousYearHoursRegisterData.Any())
            {
                previewHourRegister = true;
            }

            List<Inklokken> nextYearHoursRegisterData = await _context.Inklokkens
                           .Include(i => i.Diensten)
                           .Where(i => i.Diensten.Datum.Year == nextYear)
                           .Take(1)
                           .ToListAsync();

            if (nextYearHoursRegisterData.Any())
            {
                nextHourRegister = true;
            }


            List<WeekGroup> groupedHoursRegister = hoursRegister
                                        .GroupBy(i => GetIso8601WeekOfYear(i.Diensten.Datum))
                                        .Select(g => new WeekGroup
                                        {
                                            WeekNumber = g.Key,
                                            isComplete = g.All(i => i.Goedkeuring)
                                        })
                                        .ToList();

            List<WeekGroup> mergedWeeks = allWeeks.GroupJoin(
                groupedHoursRegister,
                allWeek => allWeek.WeekNumber,
                hourReg => hourReg.WeekNumber,
                (allWeek, hourRegs) => new WeekGroup
                {
                    WeekNumber = allWeek.WeekNumber,
                    isComplete = !hourRegs.Any() || hourRegs.All(hr => hr.isComplete),
                    Status = GetStatus(allWeek.WeekNumber, year, !hourRegs.Any() || hourRegs.All(hr => hr.isComplete), currentYear, currentWeek)
                }
                ).ToList();

            var viewModel = new WeekUrenRegistratieJaarViewModel
            {
                Year = year,
                CurrentWeek = currentWeek,
                WeekData = mergedWeeks,
                PreviousYearHourRegister = previewHourRegister,
                NextYearHourRegister = nextHourRegister
            };

            return View(viewModel);
        }



        [HttpGet]
        public IActionResult Register(int weekNumber, int year, string? dayName, string? selectedAfdeling)
        {
            //---------word niet ingevuld als ik via sql insert---------//
            var q = _context.Medewerkers.ToList();
            foreach (Medewerker man in q)
            {
                var functies = _context.Functies.ToList();
                man.Functie = functies.FirstOrDefault(f => f.FunctieId == man.FunctieId);
            }
            //---------word niet ingevuld als ik via sql insert---------//


            DateTime date = new();
            DateTime currDay = new();

            if (dayName == null || dayName.ToLower() == "maandag")
            {
                dayName = "maandag";
                date = FirstDateOfWeekISO8601(year, weekNumber);
                currDay = date;
            }
            else
            {
                date = FirstDateOfWeekISO8601(year, weekNumber);
                currDay = GetDate(date, dayName.ToLower());
            }

            var loggedInUserFiliaalId = GetLoggedInUser().FiliaalId;

            var afdelingNamen = _context.Afdelingens.Select(a => a.Naam).Distinct().ToList();

            var dienstenData = _context.Dienstens
                .Include(d => d.Medewerker)
                .Include(d => d.Inklokken)
                .Where(d => d.Datum.Date == currDay.Date && d.Medewerker.FiliaalId == loggedInUserFiliaalId && d.Inklokken.Any())
                .ToList();

            if (selectedAfdeling != null)
            {
                dienstenData = _context.Dienstens
                .Include(d => d.Medewerker)
                .Include(d => d.Inklokken)
                .Where(d => d.Datum.Date == currDay.Date && d.Medewerker.FiliaalId == loggedInUserFiliaalId && d.Inklokken.Any())
                .Where(m => m.Medewerker.Functie.Afdelings.Any(a => a.Naam == selectedAfdeling))
                .ToList();
            }
            var medewerkerPauzePerDienst = BerekenMedewerkerPauzePerDienst(dienstenData);

            bool allApproved = dienstenData.All(d => d.Inklokken.All(i => i.Goedkeuring));

            RegisterViewModel viewModel = new RegisterViewModel
            {
                CurrentDay = currDay,
                CurrentDayName = dayName,
                Year = currDay.Year,
                WeekNumber = weekNumber,
                DayOptions = GetDayOptionsForWeek(date.Year, weekNumber, dayName),
                ButtonStatuses = GetApproval(GetDayOptionsForWeek(date.Year, weekNumber, dayName)),
                Afdelingens = _context.Afdelingens.ToList(),
                FirstDayOfCurrentWeek = date,
                AfdelingNamen = afdelingNamen,
                AllApproved = allApproved,
            };

            List<RegisterViewModelItem> dienstenViewModel = new List<RegisterViewModelItem>();

            foreach (Diensten d in dienstenData)
            {
                // Check if there are Inklokken with Eind not null and DienstId matching the current Dienst
                bool hasInklokkenWithEind = d.Inklokken.Any(i => i.Eind != null && i.DienstenId == d.DienstenId);

                if (hasInklokkenWithEind)
                {
                    RegisterViewModelItem temp = new RegisterViewModelItem
                    {
                        DienstId = d.DienstenId,
                        MedewerkerNaam = $"{d.Medewerker.Voornaam} {d.Medewerker.Tussenvoegsel} {d.Medewerker.Achternaam}",
                        StartTijd = d.StartTijd,
                        EindTijd = d.EindTijd,
                        DienstTijden = FormatTijd(d.StartTijd, d.EindTijd),
                        InklokTijden = BerekenInklokTijd(d),
                        IsApproved = d.Inklokken.Any() ? d.Inklokken.All(i => i.Goedkeuring) : false,
                        HasDeviation = CheckForDeviation(d.Inklokken.Any() ? d.Inklokken.Min(i => i.Start) : TimeSpan.Zero, d.StartTijd, d.Inklokken.Any() ? d.Inklokken.Max(i => i.Eind) : TimeSpan.Zero, d.EindTijd),
                        Pauze = medewerkerPauzePerDienst.ContainsKey(d.DienstenId) ? medewerkerPauzePerDienst[d.DienstenId] : 0,
                    };
                    dienstenViewModel.Add(temp);
                }
            }

            viewModel.DienstenViewModel = dienstenViewModel.OrderBy(item => item.IsApproved).ToList();

            BerekenToeslagen(viewModel, currDay);

            SetWeekUrl(viewModel);

            return View(viewModel);
        }



        public bool CheckForDeviation(TimeSpan ingeklokteStartTijd, TimeSpan geplandeStartTijd, TimeSpan? ingeklokteEindTijd, TimeSpan geplandeEindTijd)
        {
            // Logica om afwijkingen te controleren
            bool startDeviation = (ingeklokteStartTijd < geplandeStartTijd.Subtract(TimeSpan.FromMinutes(10))) || (ingeklokteStartTijd > geplandeStartTijd.Add(TimeSpan.FromMinutes(10)));
            bool endDeviation = ingeklokteEindTijd != TimeSpan.Zero && ((ingeklokteEindTijd < geplandeEindTijd.Subtract(TimeSpan.FromMinutes(10))) || (ingeklokteEindTijd > geplandeEindTijd.Add(TimeSpan.FromMinutes(10))));

            return startDeviation || endDeviation;
        }



        private void SetWeekUrl(RegisterViewModel viewModel)
        {
            var previousWeekNumber = (viewModel.WeekNumber > 1) ? viewModel.WeekNumber - 1 : CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
                viewModel.FirstDayOfCurrentWeek.AddDays(-7), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            var previousWeekYear = (viewModel.WeekNumber > 1) ? viewModel.Year : viewModel.Year - 1;

            var nextWeekNumber = (viewModel.WeekNumber < 52) ? viewModel.WeekNumber + 1 : CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(
                viewModel.FirstDayOfCurrentWeek.AddDays(7), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            var nextWeekYear = (viewModel.WeekNumber < 52) ? viewModel.Year : viewModel.Year + 1;

            var previousWeekUrl = Url.Action("Register", "WeekUrenRegistratie", new { weekNumber = previousWeekNumber, year = previousWeekYear });
            viewModel.PreviousWeekUrl = previousWeekUrl != null ? previousWeekUrl : string.Empty;

            var nextWeekUrl = Url.Action("Register", "WeekUrenRegistratie", new { weekNumber = nextWeekNumber, year = nextWeekYear });
            viewModel.NextWeekUrl = nextWeekUrl != null ? nextWeekUrl : string.Empty;
        }




        private void BerekenToeslagen(RegisterViewModel viewModel, DateTime currDay)
        {
            foreach (var dienstenViewModel in viewModel.DienstenViewModel)
            {
                // Haal de bijbehorende dienst op
                var Dienst = _context.Dienstens.FirstOrDefault(d => d.DienstenId == dienstenViewModel.DienstId);

                // Controleer of de dienst bestaat
                if (Dienst != null)
                {
                    // Controleer of de medewerker ziek is
                    if (Dienst.Ziek)
                    {
                        // Bereken de uren dat de medewerker ziek is en rond af op 2 decimalen
                        dienstenViewModel.UrenZiek = Math.Round(CalculateTimeDifference(dienstenViewModel.StartTijd, dienstenViewModel.EindTijd), 1);
                    }
                    else
                    {
                        // Loop door de inklokken van de dienst zodat pauzes niet meetellen
                        foreach (var inklokken in Dienst.Inklokken)
                        {
                            var startTime = inklokken.Start;
                            var endTime = inklokken.Eind;

                            // Controleer of de eindtijd niet null is
                            if (endTime != null)
                            {
                                // Controleer of het zondag is of een vakantiedag
                                if (currDay.DayOfWeek == DayOfWeek.Sunday || IsHoliday(currDay))
                                {
                                    // Bereken de uren die 100% toeslag hebben en rond af op 2 decimalen
                                    dienstenViewModel.Toeslag_100 += Math.Round(CalculateTimeDifference(startTime, endTime.Value), 1);
                                }
                                else
                                {
                                    // Bereken de uren die 50% toeslag hebben en rond af op 2 decimalen
                                    dienstenViewModel.Toeslag_50 += Math.Round(CalculateOverlapHours(startTime, endTime.Value, new TimeSpan(0, 0, 0), new TimeSpan(0, 0, 6)), 1);

                                    // Controleer of het zaterdag is
                                    if (currDay.DayOfWeek == DayOfWeek.Saturday)
                                    {
                                        // Bereken de uren die 50% toeslag hebben en rond af op 2 decimalen
                                        dienstenViewModel.Toeslag_50 += Math.Round(CalculateOverlapHours(startTime, endTime.Value, new TimeSpan(18, 0, 0), new TimeSpan(24, 0, 0)), 1);
                                    }
                                    else
                                    {
                                        // Bereken de uren die 33% toeslag hebben en rond af op 2 decimalen
                                        dienstenViewModel.Toeslag_33 += Math.Round(CalculateOverlapHours(startTime, endTime.Value, new TimeSpan(20, 0, 0), new TimeSpan(21, 0, 0)), 1);

                                        // Bereken de uren die 50% toeslag hebben en rond af op 2 decimalen
                                        dienstenViewModel.Toeslag_50 += Math.Round(CalculateOverlapHours(startTime, endTime.Value, new TimeSpan(21, 0, 0), new TimeSpan(24, 0, 0)), 1);
                                    }

                                    // Bereken de uren die niet onder een toeslag vallen en rond af op 2 decimalen
                                    dienstenViewModel.Toeslag_0 += Math.Round(CalculateTimeDifference(startTime, endTime.Value) - dienstenViewModel.Toeslag_50 - dienstenViewModel.Toeslag_33, 1);
                                }
                            }
                            else
                            {
                                // Meld een fout als de eindtijd null is (dit zou normaal niet mogen gebeuren)
                                Console.WriteLine("Error: Eindtijd is null (dat mag niet)");
                            }
                        }
                    }
                }
            }
        }



        private string BerekenInklokTijd(Diensten d)
        {
            var inklokkingen = _context.Inklokkens.Where(i => i.DienstenId == d.DienstenId).ToList();

            TimeSpan? beginTijd = null;
            TimeSpan? eindTijd = null;

            foreach (Inklokken inklok in inklokkingen)
            {
                if (beginTijd == null)
                {
                    beginTijd = inklok.Start;
                }
                if (inklok.Eind != null)
                {
                    eindTijd = (TimeSpan)inklok.Eind;
                }
            }

            var tijdAsString = FormatTijd((TimeSpan)beginTijd, (TimeSpan)eindTijd);

            return tijdAsString;
        }



        private double CalculateTimeDifference(TimeSpan startTijd, TimeSpan eindTijd)
        {
            TimeSpan verschil = eindTijd - startTijd;
            return verschil.TotalHours;
        }



        //check if date is holiday
        //make this method work
        private bool IsHoliday(DateTime Date)
        {
            var loggedInUserFiliaalId = GetLoggedInUser().FiliaalId;
            return _context.Prognoses.Any(p => p.Datum == Date && p.Vakantiedag && p.FiliaalId == loggedInUserFiliaalId);
        }



        private Dictionary<DateTime, bool> GetApproval(Dictionary<DateTime, string> dictionary)
        {
            Dictionary<DateTime, bool> temp = new();
            foreach (DateTime time in dictionary.Keys)
            {
                bool flag = _context.Inklokkens.Where(m => m.Diensten.Datum.Equals(time)).All(b => b.Goedkeuring);
                temp.Add(time, flag);
            }
            return temp;
        }



        [HttpPost]
        public IActionResult ToggleApproval(int dienstId, int weekNumber, int year, string? dayName)
        {
            var inklokken = _context.Inklokkens.FirstOrDefault(d => d.DienstenId == dienstId);

            if (inklokken != null)
            {
                inklokken.Goedkeuring = !inklokken.Goedkeuring;
                _context.SaveChanges();
            }

            return RedirectToAction("Register", new { weekNumber, year, dayName });
        }



        private string FormatTijd(TimeSpan start, TimeSpan end)
        {
            return $"{start:hh\\:mm} - {end:hh\\:mm}";
        }



        private DateTime GetDate(DateTime date, string dayName)
        {
            switch (dayName)
            {
                case "dinsdag":
                    return date.AddDays(1);
                case "woensdag":
                    return date.AddDays(2);
                case "donderdag":
                    return date.AddDays(3);
                case "vrijdag":
                    return date.AddDays(4);
                case "zaterdag":
                    return date.AddDays(5);
                case "zondag":
                    return date.AddDays(6);
                default:
                    return date;
            }
        }


        private Dictionary<DateTime, string> GetDayOptionsForWeek(int year, int weekNumber, string dayName)
        {
            var dayOptions = new Dictionary<DateTime, string>();
            var firstDayOfWeek = FirstDateOfWeekISO8601(year, weekNumber);

            for (int i = 0; i < 7; i++)
            {
                var currentDay = firstDayOfWeek.AddDays(i);
                dayOptions.Add(currentDay, currentDay.ToString("dddd dd MMMM"));
            }

            return dayOptions;
        }



        private double CalculateOverlapHours(TimeSpan startTime, TimeSpan endTime, TimeSpan startLimit, TimeSpan endLimit)
        {
            TimeSpan startOverlap = startTime > startLimit ? startTime : startLimit;
            TimeSpan endOverlap = endTime < endLimit ? endTime : endLimit;

            TimeSpan overlappingHours = endOverlap - startOverlap;

            if (overlappingHours < TimeSpan.Zero)
            {
                overlappingHours = TimeSpan.Zero;
            }

            return overlappingHours.TotalHours;
        }



        private Medewerker GetLoggedInUser()
        {
            var userEmail = User.Identity.Name;

            var medewerker = _context.Medewerkers.FirstOrDefault(m => m.Email == userEmail);

            return medewerker;
        }



        private string GetStatus(int weekNumber, int Year, bool isComplete, int currentYear, int CurrentWeek)
        {
            if (isComplete && weekNumber <= CurrentWeek && Year == currentYear)
            {
                return "success";
            }
            else if (weekNumber == CurrentWeek && Year == currentYear)
            {
                return "warning";
            }
            else if (weekNumber <= CurrentWeek && Year <= currentYear || Year < currentYear)
            {
                return "danger";
            }
            else if (Year > currentYear || Year == currentYear)
            {
                return "light";
            }

            return "default";
        }


        [HttpPost]
        public async Task<IActionResult> RegisterChange(UrenWijzigenViewModel viewModel)
        {
            if (!ValidateTimes(viewModel, out string validationError))
            {
                TempData["ValidationError"] = validationError;
                // If time validation fails, call the GET method to reload the necessary data
                return await RegisterChange(viewModel.Dienst_Id, viewModel.Year, viewModel.FromPage);
            }

            if (!ModelState.IsValid)
            {
                // Handling validation errors
                foreach (var modelStateEntry in ModelState)
                {
                    string key = modelStateEntry.Key;
                    var errors = modelStateEntry.Value.Errors;

                    foreach (var error in errors)
                    {
                        Console.WriteLine($"Key: {key}, Error: {error.ErrorMessage}");
                    }
                }
                return View(viewModel);
            }

            var dienst = _context.Dienstens.FirstOrDefault(d => d.DienstenId == viewModel.Dienst_Id);

            if (dienst == null)
            {
                return NotFound();
            }

            int year = viewModel.Year;
            int weekNumber = viewModel.Week;
            string dayName = viewModel.DayName;
            int month = dienst.Datum.Month;
            int medewerkerId = dienst.MedewerkerId ?? 0;
            string afdeling = dienst.Afdelingen?.Naam ?? string.Empty;

            // Remove existing Inklokken records for this DienstenId
            var existingRecords = _context.Inklokkens.Where(i => i.DienstenId == viewModel.Dienst_Id);
            _context.Inklokkens.RemoveRange(existingRecords);

            // Always add the first Inklokken record for the DienstenId
            if (viewModel.ActualStart != TimeSpan.Zero && viewModel.ActualEnd != TimeSpan.Zero)
            {
                _context.Add(new Inklokken
                {
                    DienstenId = viewModel.Dienst_Id,
                    Start = viewModel.ActualStart,
                    Eind = viewModel.PauseStart1?.ToTimeSpan() ?? viewModel.ActualEnd,
                    Goedkeuring = true
                });
            }

            // Handle Pause 1 (skip if pause times are both at 00:00)
            if (viewModel.PauseStart1.HasValue && viewModel.PauseEnd1.HasValue &&
                !(viewModel.PauseStart1.Value == TimeOnly.MinValue && viewModel.PauseEnd1.Value == TimeOnly.MinValue))
            {
                var nextPeriodEnd = viewModel.PauseStart2.HasValue && viewModel.PauseEnd2.HasValue &&
                                    !(viewModel.PauseStart2.Value == TimeOnly.MinValue && viewModel.PauseEnd2.Value == TimeOnly.MinValue)
                                    ? viewModel.PauseStart2.Value.ToTimeSpan() : viewModel.ActualEnd;

                _context.Add(new Inklokken
                {
                    DienstenId = viewModel.Dienst_Id,
                    Start = viewModel.PauseEnd1.Value.ToTimeSpan(),
                    Eind = nextPeriodEnd,
                    Goedkeuring = true
                });
            }

            // Handle Pause 2 (skip if pause times are both at 00:00)
            if (viewModel.PauseStart2.HasValue && viewModel.PauseEnd2.HasValue &&
                !(viewModel.PauseStart2.Value == TimeOnly.MinValue && viewModel.PauseEnd2.Value == TimeOnly.MinValue))
            {
                _context.Add(new Inklokken
                {
                    DienstenId = viewModel.Dienst_Id,
                    Start = viewModel.PauseEnd2.Value.ToTimeSpan(),
                    Eind = viewModel.ActualEnd,
                    Goedkeuring = true
                });
            }

            if (dienst != null)
            {
                dienst.Ziek = viewModel.IsSick;
                _context.Update(dienst);
            }

            await _context.SaveChangesAsync();

            if (viewModel.FromPage == "Week")
            {
                return RedirectToAction("Register", new { year, weekNumber, dayName });
            }
            else
            {
                return RedirectToAction("MedewerkerRegister", "MaandUrenRegistratieController", new { currentYear = viewModel.Year, currentMonthNumber = month, afdeling = afdeling, medewerkerId = medewerkerId });
            }
        }

        private bool ValidateTimes(UrenWijzigenViewModel viewModel, out string errorMessage)
        {
            errorMessage = "";

            // Check if ActualStart is set
            if (viewModel.ActualStart == TimeSpan.Zero)
            {
                errorMessage = "Ingeklokte starttijd is niet ingesteld.";
                return false;
            }

            // Check if ActualEnd is set
            if (viewModel.ActualEnd == TimeSpan.Zero)
            {
                errorMessage = "Uitgeklokte eindtijd is niet ingesteld.";
                return false;
            }

            // Validate pauses only if they are not set to 00:00 - 00:00
            if (viewModel.PauseStart1.HasValue && viewModel.PauseEnd1.HasValue &&
                !(viewModel.PauseStart1.Value == TimeOnly.MinValue && viewModel.PauseEnd1.Value == TimeOnly.MinValue))
            {
                if (viewModel.ActualStart > viewModel.PauseStart1.Value.ToTimeSpan() ||
                    viewModel.PauseStart1.Value.ToTimeSpan() > viewModel.PauseEnd1.Value.ToTimeSpan())
                {
                    errorMessage = "Pauze 1 tijden zijn ongeldig.";
                    return false;
                }
            }

            if (viewModel.PauseStart2.HasValue && viewModel.PauseEnd2.HasValue &&
                !(viewModel.PauseStart2.Value == TimeOnly.MinValue && viewModel.PauseEnd2.Value == TimeOnly.MinValue))
            {
                if (viewModel.PauseEnd1.HasValue && viewModel.PauseEnd1.Value.ToTimeSpan() > viewModel.PauseStart2.Value.ToTimeSpan() ||
                    viewModel.PauseStart2.Value.ToTimeSpan() > viewModel.PauseEnd2.Value.ToTimeSpan())
                {
                    errorMessage = "Pauze 2 tijden zijn ongeldig.";
                    return false;
                }
            }

            // Check the overall sequence
            if (viewModel.ActualEnd < viewModel.ActualStart ||
                (viewModel.PauseEnd2.HasValue && viewModel.PauseEnd2.Value.ToTimeSpan() > viewModel.ActualEnd))
            {
                errorMessage = "De algemene tijdsvolgorde is ongeldig.";
                return false;
            }

            return true;
        }



        [HttpGet]
        public async Task<IActionResult> RegisterChange(int dienstId, int year, string fromPage)
        {
            var dienst = await _context.Dienstens
                                       .Include(d => d.Inklokken)
                                       .Include(d => d.Medewerker)
                                       .FirstOrDefaultAsync(d => d.DienstenId == dienstId);

            if (dienst == null)
            {
                return NotFound();
            }

            TimeSpan totalPause = CalcultePause(dienst.Inklokken.ToList());
            var pauseTimes = CalculatePauzeTimes(dienst.Inklokken.ToList());
            int weekNumber = GetIso8601WeekOfYear(dienst.Datum);
            foreach (var pauseTime in pauseTimes)
            {
                Console.WriteLine(pauseTime.Start + "-" + pauseTime.End);
            }
            string fullName = dienst.Medewerker != null
    ? $"{dienst.Medewerker.Voornaam} {(string.IsNullOrEmpty(dienst.Medewerker.Tussenvoegsel) ? "" : $"{dienst.Medewerker.Tussenvoegsel} ")}{dienst.Medewerker.Achternaam}"
    : string.Empty;

            var viewModel = new UrenWijzigenViewModel
            {
                Dienst_Id = dienstId,
                Name = fullName,
                Date = dienst.Datum.ToString("dddd, dd MMMM yyyy", new CultureInfo("nl-NL")),
                Year = year,
                Week = weekNumber,
                ScheduledStart = dienst.StartTijd,
                ScheduledEnd = dienst.EindTijd,
                ActualStart = dienst.Inklokken.Any() ? dienst.Inklokken.Min(i => i.Start) : TimeSpan.Zero,
                ActualEnd = dienst.Inklokken.Any(i => i.Eind.HasValue) ? dienst.Inklokken.Max(i => i.Eind.Value) : TimeSpan.Zero,
                BreakLength = totalPause,
                PauseStart1 = pauseTimes.ElementAtOrDefault(0)?.Start ?? new TimeOnly(0, 0),
                PauseEnd1 = pauseTimes.ElementAtOrDefault(0)?.End ?? new TimeOnly(0, 0),
                PauseStart2 = pauseTimes.ElementAtOrDefault(1)?.Start ?? new TimeOnly(0, 0),
                PauseEnd2 = pauseTimes.ElementAtOrDefault(1)?.End ?? new TimeOnly(0, 0),
                IsSick = dienst.Ziek,
                DayName = dienst.Datum.ToString("dddd"),
                FromPage = fromPage
            };

            return View("UrenWijzigen", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> BackButton(UrenWijzigenViewModel viewModel)
        {
            var dienst = await _context.Dienstens
                                       .Include(d => d.Medewerker)
                                       .Include(d => d.Afdelingen)
                                       .FirstOrDefaultAsync(d => d.DienstenId == viewModel.Dienst_Id);

            if (dienst == null)
            {
                return NotFound();
            }

            int month = dienst.Datum.Month;
            int medewerkerId = dienst.MedewerkerId ?? 0;
            string afdeling = dienst.Afdelingen?.Naam ?? string.Empty;

            if (viewModel.FromPage == "Week")
            {
                return RedirectToAction("Register", "WeekUrenRegistratie", new { year = viewModel.Year, weekNumber = viewModel.Week, dayName = viewModel.DayName });
            }
            else
            {
                return RedirectToAction("MedewerkerRegister", "MaandUrenRegistratie", new { currentYear = viewModel.Year, currentMonthNumber = month, afdeling = afdeling, medewerkerId = medewerkerId });
            }
        }


        private List<InklokTime> CalculatePauzeTimes(List<Inklokken> inklokkenData)
        {
            List<InklokTime> pauzes = new List<InklokTime>();

            inklokkenData = inklokkenData.OrderBy(inklok => inklok.Start).ToList();

            for (int i = 0; i < inklokkenData.Count - 1; i++)
            {
                var currentInklokken = inklokkenData[i];
                var nextInklokken = inklokkenData[i + 1];

                if (currentInklokken.Eind.HasValue)
                {
                    pauzes.Add(new InklokTime
                    {
                        InklokkenId = currentInklokken.InklokkenId,
                        Start = new TimeOnly(currentInklokken.Eind.Value.Hours, currentInklokken.Eind.Value.Minutes),
                        End = new TimeOnly(nextInklokken.Start.Hours, nextInklokken.Start.Minutes)
                    });
                }
            }

            return pauzes;
        }




        private TimeSpan CalcultePause(List<Inklokken> inklokken)
        {
            TimeSpan totalPauze = TimeSpan.Zero;

            for (int i = 0; i < inklokken.Count - 1; i++)
            {
                var currentEnd = inklokken[i].Eind;
                var nextStart = inklokken[i + 1].Start;
                if (currentEnd.HasValue && nextStart > currentEnd.Value)
                {
                    totalPauze += nextStart - currentEnd.Value;
                }
            }

            return totalPauze;
        }


        private Dictionary<int, int> BerekenMedewerkerPauzePerDienst(List<Diensten> dienstenData)

        {

            var medewerkerPauzePerDienst = new Dictionary<int, int>();

            foreach (var dienst in dienstenData)
            {
                List<Inklokken> inklok = _context.Inklokkens.Where(i => i.DienstenId == dienst.DienstenId).ToList();
                var dienstPauze = 0;

                for (int i = 0; i < (inklok.Count - 1); i++)
                {
                    dienstPauze += ((int)((TimeSpan)inklok[i].Eind - inklok[i + 1].Start).TotalMinutes * -1);
                }

                medewerkerPauzePerDienst[dienst.DienstenId] = dienstPauze;
            }

            return medewerkerPauzePerDienst;
        }


        private int GetIso8601WeekOfYear(DateTime date)
        {
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(date);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                date = date.AddDays(3);
            }
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        private DateTime FirstDateOfWeekISO8601(int year, int weekNumber)
        {
            DateTime januaryFirst = new DateTime(year, 1, 1);
            DateTime firstDayOfYear = januaryFirst.AddDays(-((int)januaryFirst.DayOfWeek - 1));

            int daysToFirstCorrectWeek = (weekNumber - 1) * 7;
            DateTime firstDayOfWeek = firstDayOfYear.AddDays(daysToFirstCorrectWeek);

            return firstDayOfWeek;
        }

    }
}
