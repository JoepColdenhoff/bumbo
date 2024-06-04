using Bumbo.Models;
using Bumbo.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bumbo.Controllers
{
    [Authorize(Roles = "Manager")]
    public class MaandUrenRegistratieController : Controller
    {
        private readonly BumboContext _context;

        public MaandUrenRegistratieController(BumboContext context)
        {
            _context = context;
        }



        public IActionResult ToggleApproval(int dienstId, int currentYear, int currentMonthNumber, string afdeling, int medewerkerId)
        {
            var inklokken = _context.Inklokkens.FirstOrDefault(d => d.DienstenId == dienstId);

            if (inklokken != null)
            {
                inklokken.Goedkeuring = !inklokken.Goedkeuring;
                _context.SaveChanges();
            }

            return RedirectToAction("MedewerkerRegister", new { currentMonthNumber, currentYear, afdeling, medewerkerId });
        }



        [HttpGet]
        public async Task<IActionResult> Index(int year)
        {
            int currentYear = DateTime.Now.Year;
            int currentMonth = DateTime.Now.Month;
            int previousYear = year - 1;
            int nextYear = year + 1;
            bool previewHourRegisterYearData = false;
            bool nextYearHourRegisterYearData = false;

            List<MonthGroup> monthlyGroups = Enumerable.Range(1, 12)
                                            .Select(month => new MonthGroup
                                            {
                                                Month = month,
                                                isComplete = true
                                            })
                                            .ToList();

            List<Inklokken> hoursRegister = await _context.Inklokkens
                                    .Include(i => i.Diensten)
                                    .Where(i => i.Diensten.Datum.Year == year)
                                    .ToListAsync();

            if (!hoursRegister.Any() && year < currentYear || !hoursRegister.Any() && year > currentYear)
            {
                TempData["TempData"] = $"Er bestaan geen uren uit het jaar <b>{year}</b>";
                return RedirectToAction("Index", new { Year = currentYear });
            }

            List<Inklokken> previousHoursRegisterYear = await _context.Inklokkens
                        .Include(i => i.Diensten)
                        .Where(i => i.Diensten.Datum.Year == previousYear)
                        .Take(1)
                        .ToListAsync();

            if (previousHoursRegisterYear.Any())
            {
                previewHourRegisterYearData = true;
            }

            List<Inklokken> nextYearHoursRegisterYear = await _context.Inklokkens
                        .Include(i => i.Diensten)
                        .Where(i => i.Diensten.Datum.Year == nextYear)
                        .Take(1)
                        .ToListAsync();

            if (nextYearHoursRegisterYear.Any())
            {
                nextYearHourRegisterYearData = true;
            }


            List<MonthGroup> groupedHoursRegister = hoursRegister
                                                    .GroupBy(i => new { Year = i.Diensten.Datum.Year, Month = i.Diensten.Datum.Month })
                                                    .Select(g => new MonthGroup
                                                    {
                                                        //Year = g.Key.Year,
                                                        Month = g.Key.Month,
                                                        isComplete = g.All(i => i.Goedkeuring)
                                                    })
                                                        .ToList();

            List<MonthGroup> mergedMonths = monthlyGroups.GroupJoin(
                                            groupedHoursRegister,
                                            monthGroup => monthGroup.Month,
                                            hourRegGroup => hourRegGroup.Month,
                                            (monthGroup, hourRegGroups) => new MonthGroup
                                            {
                                                Month = monthGroup.Month,
                                                isComplete = !hourRegGroups.Any() ? monthGroup.isComplete : hourRegGroups.All(hr => hr.isComplete)
                                            }
                                            ).ToList();

            var viewModel = new MaandUrenRegistratieJaarViewModel
            {
                Year = year,
                CurrentMonth = currentMonth,
                Months = mergedMonths,
                PreviousYearMonthsRegister = previewHourRegisterYearData,
                NextYearMonthRegister = nextYearHourRegisterYearData
            };

            return View(viewModel);
        }



        [HttpGet]
        public IActionResult MedewerkerRegister(int currentYear, int currentMonthNumber, string afdeling, int medewerkerId)
        {
            Medewerker medewerker = _context.Medewerkers.FirstOrDefault(m => m.MedewerkerId == medewerkerId);

            DateTime minDate = GetFirstDayOfMonth(currentYear, currentMonthNumber);
            DateTime maxDate = GetLastDayOfMonth(currentYear, currentMonthNumber);

            List<SpecifiekMedewerkerRegisterViewModel> mModel = new List<SpecifiekMedewerkerRegisterViewModel>();

            MedewerkerRegisterViewModel model = new MedewerkerRegisterViewModel
            {
                Medewerker = medewerker,
                CurrentAfdeling = afdeling,
                CurrentMonthName = GetMonthAsString(currentMonthNumber),
                Year = currentYear,
                CurrentMonthNumber = currentMonthNumber
            };

            List<Diensten> diensten = _context.Dienstens
                .Where(m => m.MedewerkerId == medewerkerId && m.Datum >= minDate && m.Datum <= maxDate)
                .ToList();

            foreach (Diensten dienst in diensten)
            {
                var inklokkens = _context.Inklokkens.FirstOrDefault(i => i.DienstenId == dienst.DienstenId && i.Diensten.Medewerker == medewerker);

                SpecifiekMedewerkerRegisterViewModel temp = new SpecifiekMedewerkerRegisterViewModel
                {
                    DienstId = dienst.DienstenId,
                    IsApproved = inklokkens?.Goedkeuring ?? false,
                    Datum = dienst.Datum.ToString("dd dddd"),
                    Gepland = FormatTijd(dienst.StartTijd, dienst.EindTijd),
                    Geklokt = FormatTijdInklokken(dienst),
                    Pauze = BerekenMedewerkerPauzePerDienst(dienst),
                    HasDeviation = CheckForDeviation(
                        dienst.Inklokken.Any() ? dienst.Inklokken.Min(i => i.Start) : TimeSpan.Zero,
                        dienst.StartTijd,
                        dienst.Inklokken.Any() ? dienst.Inklokken.Max(i => i.Eind) : TimeSpan.Zero,
                        dienst.EindTijd),
                    Afdeling = _context.Afdelingens
                        .Where(a => a.AfdelingId == dienst.AfdelingId)
                        .Select(a => a.Naam)
                        .FirstOrDefault()
                };

                mModel.Add(temp);
            }
            
            model.Rijen = mModel;
            BerekenToeslagen(model);

            return View(model);
        }

        private string FormatTijdInklokken(Diensten d)
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

        [HttpGet]
        public IActionResult Register(int monthNumber, int year, string? selectedAfdeling, string? message)
        {
            //---------word niet ingevuld als ik via sql insert---------//
            var q = _context.Medewerkers.ToList();
            foreach (Medewerker man in q)
            {
                var functies = _context.Functies.ToList();
                man.Functie = functies.FirstOrDefault(f => f.FunctieId == man.FunctieId);
            }
            //---------word niet ingevuld als ik via sql insert---------//

            if (selectedAfdeling == null)
            {
                selectedAfdeling = "Vers";
            }

            //voor de verzend message
            if (message != null)
            {
                TempData["SuccessMessage"] = "Verzenden is gelukt!";
            }

            DateTime minDate = GetFirstDayOfMonth(year, monthNumber);
            DateTime maxDate = GetLastDayOfMonth(year, monthNumber);

            var medewerkers = _context.Medewerkers
                .Include(m => m.Dienstens)
                .ThenInclude(d => d.Inklokken)
                .Where(m => m.FiliaalId == GetLoggedInUser().FiliaalId)
                .Where(m => m.Dienstens.Any(d => d.Datum >= minDate && d.Datum <= maxDate))
                .Where(m => m.Functie.Afdelings.Any(a => a.Naam == selectedAfdeling))
                .ToList();

            var afdelingNamen = _context.Afdelingens.Select(a => a.Naam).Distinct().ToList();

            int previousMonth = (monthNumber > 1) ? monthNumber - 1 : 12;
            int previousYear = (monthNumber > 1) ? year : year - 1;

            int nextMonth = (monthNumber < 12) ? monthNumber + 1 : 1;
            int nextYear = (monthNumber < 12) ? year : year + 1;

            MaandRegisterDataViewModel data = new MaandRegisterDataViewModel()
            {

                maandRegisterViewModels = new(),
                Year = year,
                CurrentMonthNumber = monthNumber,
                PreviousMonthUrl = Url.Action("Register", "MaandUrenRegistratie", new { monthNumber = previousMonth, year = previousYear }),
                NextMonthUrl = Url.Action("Register", "MaandUrenRegistratie", new { monthNumber = nextMonth, year = nextYear })
            };

            foreach (var m in medewerkers)
            {
                TimeSpan? totaal = null;
                var inklokkens = _context.Inklokkens.Where(b => b.Diensten.MedewerkerId == m.MedewerkerId).Where(a=> a.Goedkeuring == true).ToList();

                foreach (var klok in inklokkens)
                {
                    var tempTotaal = klok.Eind - klok.Start;
                    totaal = (totaal ?? TimeSpan.Zero) + tempTotaal;
                }

                MaandRegisterViewModel model = new MaandRegisterViewModel
                {
                    Medewerker = m,
                    TotaalUren = (int)totaal.Value.TotalHours,
                };

                data.maandRegisterViewModels.Add(model);
            }

            if (data.maandRegisterViewModels != null || data.maandRegisterViewModels.Any())
            {
                BerekenToeslag(data);
            }

            data.CurrentAfdeling = selectedAfdeling;
            data.CurrentMonthName = GetMonthAsString(monthNumber);
            data.AfdelingNamen = afdelingNamen;

            return View(data);
        }



        public IActionResult MaandSwitch(int currentYear, int currentMonthNumber, string afdeling, int medewerkerId, bool change)
        {
            if (change)
            {
                if (currentMonthNumber != 12)
                {
                    currentMonthNumber += 1;
                }
                else
                {
                    currentMonthNumber = 1;
                    currentYear += 1;
                }
            }
            else
            {
                if (currentMonthNumber != 1)
                {
                    currentMonthNumber -= 1;
                }
                else
                {
                    currentMonthNumber = 12;
                    currentYear -= 1;
                }
            }
            return RedirectToAction("MedewerkerRegister", new { currentMonthNumber, currentYear, afdeling, medewerkerId });
        }


        private void BerekenToeslagen(MedewerkerRegisterViewModel viewModel)
        {
            foreach (var dienstenViewModel in viewModel.Rijen)
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
                        dienstenViewModel.Toeslag_70 = Math.Round(CalculateTimeDifference(Dienst.StartTijd, Dienst.EindTijd), 1);
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
                                if (Dienst.Datum.DayOfWeek == DayOfWeek.Sunday || IsHoliday(Dienst.Datum))
                                {
                                    // Bereken de uren die 100% toeslag hebben en rond af op 2 decimalen
                                    dienstenViewModel.Toeslag_100 += Math.Round(CalculateTimeDifference(startTime, endTime.Value), 1);
                                }
                                else
                                {
                                    // Bereken de uren die 50% toeslag hebben en rond af op 2 decimalen
                                    dienstenViewModel.Toeslag_50 += Math.Round(CalculateOverlapHours(startTime, endTime.Value, new TimeSpan(0, 0, 0), new TimeSpan(0, 0, 6)), 1);

                                    // Controleer of het zaterdag is
                                    if (Dienst.Datum.DayOfWeek == DayOfWeek.Saturday)
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



        private void BerekenToeslag(MaandRegisterDataViewModel data)
        {
            foreach (var registerVM in data.maandRegisterViewModels)
            {
                // Haal de bijbehorende diensten op
                var inklokkens = _context.Dienstens.Where(b => b.MedewerkerId == registerVM.Medewerker.MedewerkerId).ToList();

                // Initialiseer de variabelen voor de uiteindelijke totalen
                double Toeslag_0_final = 0;
                double Toeslag_33_final = 0;
                double Toeslag_50_final = 0;
                double Toeslag_70_final = 0;
                double Toeslag_100_final = 0;

                foreach (Diensten Dienst in inklokkens)
                {
                    // Initialiseer variabelen voor elke dienst
                    double Toeslag_0 = 0;
                    double Toeslag_33 = 0;
                    double Toeslag_50 = 0;
                    double Toeslag_70 = 0;
                    double Toeslag_100 = 0;

                    // Controleer of de dienst bestaat
                    if (Dienst != null)
                    {
                        // Controleer of de medewerker ziek is
                        if (Dienst.Ziek)
                        {
                            // Bereken de uren dat de medewerker ziek is en rond af op 2 decimalen
                            Toeslag_70 += Math.Round(CalculateTimeDifference(Dienst.StartTijd, Dienst.EindTijd), 2);
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
                                    if (Dienst.Datum.DayOfWeek == DayOfWeek.Sunday || IsHoliday(Dienst.Datum))
                                    {
                                        // Bereken de uren die 100% toeslag hebben en rond af op 2 decimalen
                                        Toeslag_100 += Math.Round(CalculateTimeDifference(startTime, endTime.Value), 2);
                                    }
                                    else
                                    {
                                        // Bereken de uren die 50% toeslag hebben en rond af op 2 decimalen
                                        Toeslag_50 += Math.Round(CalculateOverlapHours(startTime, endTime.Value, new TimeSpan(0, 0, 0), new TimeSpan(0, 0, 6)), 2);

                                        // Controleer of het zaterdag is
                                        if (Dienst.Datum.DayOfWeek == DayOfWeek.Saturday)
                                        {
                                            // Bereken de uren die 50% toeslag hebben en rond af op 2 decimalen
                                            Toeslag_50 += Math.Round(CalculateOverlapHours(startTime, endTime.Value, new TimeSpan(18, 0, 0), new TimeSpan(24, 0, 0)), 2);
                                        }
                                        else
                                        {
                                            // Bereken de uren die 33% toeslag hebben en rond af op 2 decimalen
                                            Toeslag_33 += Math.Round(CalculateOverlapHours(startTime, endTime.Value, new TimeSpan(20, 0, 0), new TimeSpan(21, 0, 0)), 2);

                                            // Bereken de uren die 50% toeslag hebben en rond af op 2 decimalen
                                            Toeslag_50 += Math.Round(CalculateOverlapHours(startTime, endTime.Value, new TimeSpan(21, 0, 0), new TimeSpan(24, 0, 0)), 2);
                                        }

                                        // Bereken de uren die niet onder een toeslag vallen en rond af op 2 decimalen
                                        Toeslag_0 += Math.Round(CalculateTimeDifference(startTime, endTime.Value) - Toeslag_50 - Toeslag_33, 2);
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

                    // Update de totale toeslag voor deze dienst
                    Toeslag_0_final += Toeslag_0;
                    Toeslag_33_final += Toeslag_33;
                    Toeslag_50_final += Toeslag_50;
                    Toeslag_70_final += Toeslag_70;
                    Toeslag_100_final += Toeslag_100;
                }

                // Update de totale toeslagen voor de huidige medewerker
                registerVM.ToeslagUur0 = Toeslag_0_final;
                registerVM.ToeslagUur33 = Toeslag_33_final;
                registerVM.ToeslagUur50 = Toeslag_50_final;
                registerVM.ToeslagUur70 = Toeslag_70_final;
                registerVM.ToeslagUur100 = Toeslag_100_final;
            }
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



        private double CalculateTimeDifference(TimeSpan startTijd, TimeSpan eindTijd)
        {
            TimeSpan verschil = eindTijd - startTijd;
            return verschil.TotalHours;
        }



        private bool IsHoliday(DateTime Date)
        {
            var loggedInUserFiliaalId = GetLoggedInUser().FiliaalId;
            return _context.Prognoses.Any(p => p.Datum == Date && p.Vakantiedag && p.FiliaalId == loggedInUserFiliaalId);
        }



        private string GetMonthAsString(int monthNumber)
        {
            switch (monthNumber)
            {
                case 1: return "januari";
                case 2: return "februari";
                case 3: return "maart";
                case 4: return "april";
                case 5: return "mei";
                case 6: return "juni";
                case 7: return "juli";
                case 8: return "augustus";
                case 9: return "september";
                case 10: return "oktober";
                case 11: return "november";
                case 12: return "december";
                default: return "Ongeldige maand";
            }
        }



        private DateTime GetFirstDayOfMonth(int year, int month)
        {
            return new DateTime(year, month, 1);
        }



        private DateTime GetLastDayOfMonth(int year, int month)
        {
            return new DateTime(year, month, 1).AddMonths(1).AddDays(-1);
        }



        private Medewerker GetLoggedInUser()
        {
            var userEmail = User.Identity.Name;

            var medewerker = _context.Medewerkers.FirstOrDefault(m => m.Email == userEmail);

            return medewerker;
        }



        private string FormatTijd(TimeSpan start, TimeSpan end)
        {
            return $"{start:hh\\:mm} - {end:hh\\:mm}";
        }



        private int BerekenMedewerkerPauzePerDienst(Diensten dienst)
        {
              List<Inklokken> inklok = _context.Inklokkens.Where(i => i.DienstenId == dienst.DienstenId).ToList();
                var dienstPauze = 0;

                for (int i = 0; i < (inklok.Count - 1); i++)
                {
                    dienstPauze += ((int)((TimeSpan)inklok[i].Eind - inklok[i + 1].Start).TotalMinutes * -1);
                }
                            
            
            return dienstPauze; 
        }



        private bool CheckForDeviation(TimeSpan ingeklokteStartTijd, TimeSpan geplandeStartTijd, TimeSpan? ingeklokteEindTijd, TimeSpan geplandeEindTijd)
        {

            bool startDeviation = (ingeklokteStartTijd < geplandeStartTijd.Subtract(TimeSpan.FromMinutes(10)))
                || (ingeklokteStartTijd > geplandeStartTijd.Add(TimeSpan.FromMinutes(10)));
            bool endDeviation = ingeklokteEindTijd != TimeSpan.Zero && ((ingeklokteEindTijd < geplandeEindTijd.Subtract(TimeSpan.FromMinutes(10))) ||
                (ingeklokteEindTijd > geplandeEindTijd.Add(TimeSpan.FromMinutes(10))));

            return startDeviation || endDeviation;
        }
    }
}
