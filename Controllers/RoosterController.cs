using Bumbo.Models;
using Bumbo.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using static Bumbo.ViewModels.RoosterViewStructs;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Bumbo.Controllers
{
    public class RoosterController : Controller
    {
        private readonly BumboContext _context;
        private CAOController CAOController { get; set; }

        public RoosterController(BumboContext context)
        {
            _context = context;
            CAOController = new CAOController(context, this);
        }
        public Medewerker GetLoggedInUser()
        {
            var userEmail = User.Identity.Name;

            var medewerker = _context.Medewerkers.FirstOrDefault(m => m.Email == userEmail);

            return medewerker;
        }

        private List<DepartmentOption> GetDepartmentOptions()
        {
            return _context.Afdelingens.Select(a => new DepartmentOption
            {
                DepartmentId = a.AfdelingId,
                Name = a.Naam
            }).ToList();
        }

        public IActionResult RoosterCreate(int year, int weekNumber)
        {
            /*
            HttpContext.Session.Remove("SelectedYear");
            HttpContext.Session.Remove("SelectedWeekNumber");
            HttpContext.Session.Remove("SelectedDate");
            HttpContext.Session.Remove("SelectedDepartment");*/

            var viewModel = new RoosteringViewModel
            {
                year = year,
                WeekNumber = weekNumber,
                DayOptions = GetDayOptionsForWeek(year, weekNumber),
                SelectedDate = GetFirstDayOfWeek(year, weekNumber),
                DepartmentOptions = GetDepartmentOptions()
            };

            // Use existing session values if they are set
            viewModel.SelectedDate = HttpContext.Session.GetString("SelectedDate") != null
                ? DateTime.Parse(HttpContext.Session.GetString("SelectedDate"))
                : GetFirstDayOfWeek(year, weekNumber);

            viewModel.SelectedDepartment = HttpContext.Session.GetInt32("SelectedDepartment")
                ?? viewModel.DepartmentOptions.FirstOrDefault().DepartmentId;

            // Fetch and assign data based on these selections
            viewModel.AvailableAvailabilities = FetchAvailableEmployees(viewModel.SelectedDate, viewModel.SelectedDepartment);
            viewModel.ScheduledAvailabilities = FetchScheduledEmployees(viewModel.SelectedDate, viewModel.SelectedDepartment);
            viewModel.PrognoseUren = FetchPrognose(viewModel.SelectedDate, viewModel.SelectedDepartment);
            viewModel.scheduledHours = GetTotalScheduledHours(FetchScheduledEmployees(viewModel.SelectedDate, viewModel.SelectedDepartment));

            return View(viewModel);
        }


        /*
        public IActionResult RoosterCreate(int year, int weekNumber)
        {
            HttpContext.Session.Remove("SelectedYear");
            HttpContext.Session.Remove("SelectedWeekNumber");
            HttpContext.Session.Remove("SelectedDate");
            HttpContext.Session.Remove("SelectedDepartment");

            var viewModel = new RoosteringViewModel
            {
                year = year,
                WeekNumber = weekNumber,
                DayOptions = GetDayOptionsForWeek(year, weekNumber),
                SelectedDate = GetFirstDayOfWeek(year, weekNumber),
                DepartmentOptions = GetDepartmentOptions()
            };

            if (viewModel.SelectedDate == default(DateTime))
            {
                viewModel.SelectedDate = viewModel.DayOptions.First().Date;
                viewModel.SelectedDepartment = viewModel.DepartmentOptions.First().DepartmentId;
                // Fetch and assign data based on these selections
            }

            if (DateTime.Parse(HttpContext.Session.GetString("SelectedDate") ?? DateTime.Now.ToString("o")) != null)
            {
                viewModel.AvailableAvailabilities = FetchAvailableEmployees(DateTime.Parse(HttpContext.Session.GetString("SelectedDate") ?? DateTime.Now.ToString("o")), HttpContext.Session.GetInt32("SelectedDepartment") ?? default);
                viewModel.ScheduledAvailabilities = FetchScheduledEmployees(DateTime.Parse(HttpContext.Session.GetString("SelectedDate") ?? DateTime.Now.ToString("o")), HttpContext.Session.GetInt32("SelectedDepartment") ?? default);
                viewModel.PrognoseUren = FetchPrognose(DateTime.Parse(HttpContext.Session.GetString("SelectedDate") ?? DateTime.Now.ToString("o")), HttpContext.Session.GetInt32("SelectedDepartment") ?? default);
                viewModel.scheduledHours = GetTotalScheduledHours(FetchScheduledEmployees(DateTime.Parse(HttpContext.Session.GetString("SelectedDate") ?? DateTime.Now.ToString("o")), HttpContext.Session.GetInt32("SelectedDepartment") ?? default));
            }

            return View(viewModel);
        }*/


        [HttpPost]
        public IActionResult RoosterCreate(RoosteringViewModel model, string SelectedDate, int SelectedDepartment)
        {
            // Parse the selected date and department and update the model
            if (!string.IsNullOrEmpty(SelectedDate))
            {
                model.SelectedDate = DateTime.Parse(SelectedDate);
            }
            model.SelectedDepartment = SelectedDepartment;

            // Repopulate DayOptions and DepartmentOptions
            model.DayOptions = GetDayOptionsForWeek(model.year, model.WeekNumber);
            model.DepartmentOptions = GetDepartmentOptions();

            // Fetch available and scheduled availabilities, prognose uren, and scheduled hours
            model.AvailableAvailabilities = FetchAvailableEmployees(model.SelectedDate, model.SelectedDepartment);
            model.ScheduledAvailabilities = FetchScheduledEmployees(model.SelectedDate, model.SelectedDepartment);
            model.PrognoseUren = FetchPrognose(model.SelectedDate, model.SelectedDepartment);
            model.scheduledHours = GetTotalScheduledHours(FetchScheduledEmployees(model.SelectedDate, model.SelectedDepartment));

            // Set session variables
            HttpContext.Session.SetInt32("SelectedYear", model.year);
            HttpContext.Session.SetInt32("SelectedWeekNumber", model.WeekNumber);
            HttpContext.Session.SetString("SelectedDate", model.SelectedDate.ToString("o"));
            HttpContext.Session.SetInt32("SelectedDepartment", model.SelectedDepartment);

            return View(model);
        }


        private TimeSpan GetTotalScheduledHours(List<Diensten> scheduledAvailabilities)
        {
            TimeSpan totalScheduledHours = TimeSpan.Zero;
            foreach (var dienst in scheduledAvailabilities)
            {
                TimeSpan duration = dienst.EindTijd - dienst.StartTijd;
                totalScheduledHours += duration;
            }
            return totalScheduledHours;
        }

        public IActionResult ClearSessionAndRedirect()
        {
            HttpContext.Session.Remove("SelectedYear");
            HttpContext.Session.Remove("SelectedWeekNumber");
            HttpContext.Session.Remove("SelectedDate");
            HttpContext.Session.Remove("SelectedDepartment");

            return RedirectToAction("Index", "RoosterManager", new { year = DateTime.Now.Year });
        }


        private List<DayOption> GetDayOptionsForWeek(int year, int weekNumber)
        {
            var dayOptions = new List<DayOption>();

            var firstDayOfWeek = FirstDateOfWeekISO8601(year, weekNumber);
            for (int i = 0; i < 7; i++)
            {
                var day = firstDayOfWeek.AddDays(i);
                var formattedDay = day.ToString("dddd dd/MM", new CultureInfo("nl-NL"));
                dayOptions.Add(new DayOption { Date = day, Display = formattedDay });
            }

            return dayOptions;
        }



        private DateTime FirstDateOfWeekISO8601(int year, int weekOfYear)
        {
            DateTime jan1 = new DateTime(year, 1, 1);

            int daysOffset = DayOfWeek.Monday - jan1.DayOfWeek;
            if (daysOffset > 0)
            {
                daysOffset -= 7;
            }

            DateTime firstMonday = jan1.AddDays(daysOffset);

            var cal = CultureInfo.CurrentCulture.Calendar;
            int firstWeek = cal.GetWeekOfYear(jan1, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            if (firstWeek == 1)
            {
                weekOfYear -= 1;
            }

            return firstMonday.AddDays(weekOfYear * 7);
        }


        private DateTime GetFirstDayOfWeek(int year, int weekNumber)
        {
            return FirstDateOfWeekISO8601(year, weekNumber);
        }

        private List<Beschikbaarheid> FetchAvailableEmployees(DateTime date, int departmentId)
        {
            var loggedInUser = GetLoggedInUser();
            if (loggedInUser == null || loggedInUser.FiliaalId == null)
            {
                return new List<Beschikbaarheid>();
            }

            var assignedBeschikbaarheidIds = _context.Dienstens
                                                     .Where(d => d.Datum.Date == date.Date)
                                                     .Select(d => d.BeschikbaarheidId)
                                                     .ToList();

            var availableEmployees = _context.Beschikbaarheids
                                             .Include(b => b.Medewerker)
                                             .ThenInclude(m => m.Functie)
                                             .ThenInclude(f => f.Afdelings)
                                             .Where(b => b.Datum.Date == date.Date
                                                         && b.Medewerker.Functie.Afdelings.Any(a => a.AfdelingId == departmentId)
                                                         && b.Medewerker.FiliaalId == loggedInUser.FiliaalId
                                                         && !assignedBeschikbaarheidIds.Contains(b.BeschikbaarheidId))
                                             .ToList();
            return availableEmployees;
        }

        private List<Diensten> FetchScheduledEmployees(DateTime date, int departmentId)
        {
            var loggedInUser = GetLoggedInUser();
            if (loggedInUser == null || loggedInUser.FiliaalId == null)
            {
                return new List<Diensten>();
            }

            return _context.Dienstens
                           .Include(d => d.Medewerker)
                           .Where(d => d.Datum == date
                                       && d.Medewerker.Functie.Afdelings.Any(a => a.AfdelingId == departmentId)
                                       && d.Medewerker.FiliaalId == loggedInUser.FiliaalId
                                       && d.AfdelingId == departmentId)
                           .ToList();
        }

        private int getMedewerkerFromBeschikbaarheidId(int beschikbaarheidId)
        {
            Beschikbaarheid beschikbaarheid = _context.Beschikbaarheids.FirstOrDefault(b => b.BeschikbaarheidId == beschikbaarheidId);

            Medewerker medewerker = _context.Medewerkers.FirstOrDefault(m => m.MedewerkerId == beschikbaarheid.MedewerkerId);

            return medewerker.MedewerkerId;
        }


        [HttpPost]
        public IActionResult AddToRoster(int beschikbaarheidId, TimeSpan startTijd, TimeSpan endTijd)
        {
            var beschikbaarheid = _context.Beschikbaarheids
                                          .FirstOrDefault(b => b.BeschikbaarheidId == beschikbaarheidId);
            int selectedMedewerkerId = getMedewerkerFromBeschikbaarheidId(beschikbaarheidId);

            if (startTijd >= beschikbaarheid.StartTijd && endTijd <= beschikbaarheid.EindTijd && startTijd < endTijd)
            {
                

                var dienst = new Diensten
                {
                    MedewerkerId = beschikbaarheid.MedewerkerId,
                    StartTijd = startTijd,
                    EindTijd = endTijd,
                    Datum = beschikbaarheid.Datum,
                    BeschikbaarheidId = beschikbaarheid.BeschikbaarheidId,
                    AfdelingId = HttpContext.Session.GetInt32("SelectedDepartment") ?? default
                };

                _context.Dienstens.Add(dienst);
                _context.SaveChanges();
            }
            else
            {
                var years = HttpContext.Session.GetInt32("SelectedYear") ?? default;
                var weekNumbers = HttpContext.Session.GetInt32("SelectedWeekNumber") ?? default;
                var selectedDates = DateTime.Parse(HttpContext.Session.GetString("SelectedDate") ?? DateTime.Now.ToString("o"));
                var selectedDepartments = HttpContext.Session.GetInt32("SelectedDepartment") ?? default;

                TempData["ErrorName"] = "UpdateWorkingHoursError";
                TempData["ErrorMessage"] = "Uren moeten binnen beschikbaarheid zijn en/of starttijd kleiner dan eindtijd";

                return RedirectToAction("RoosterCreate", new { year = years, weekNumber = weekNumbers, selectedDate = selectedDates, selectedDepartment = selectedDepartments });
            }
            var dienstId = _context.Dienstens.FirstOrDefault(d => d.BeschikbaarheidId == beschikbaarheidId);
            CAOController.CheckAgeShiftRules(selectedMedewerkerId, dienstId.DienstenId);

            int year = HttpContext.Session.GetInt32("SelectedYear") ?? DateTime.Now.Year;
            int weekNumber = HttpContext.Session.GetInt32("SelectedWeekNumber") ?? CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            DateTime selectedDate = DateTime.Parse(HttpContext.Session.GetString("SelectedDate") ?? DateTime.Now.ToString("o"));
            int selectedDepartment = HttpContext.Session.GetInt32("SelectedDepartment") ?? 1; 

            return RedirectToAction("RoosterCreate", new { year, weekNumber, SelectedDate = selectedDate.ToString("yyyy-MM-dd"), SelectedDepartment = selectedDepartment });
        }
        
        public IActionResult RoosterOverview(int year, int weekNumber)
        {
            RoosterOverviewViewModel overview = new RoosterOverviewViewModel
            {
                year = year,
                WeekNumber = weekNumber,
                DayOptions = GetDayOptionsForWeek(year, weekNumber),
                SelectedDate = GetFirstDayOfWeek(year, weekNumber)
            };

            if (overview.SelectedDate == default(DateTime) && overview.DayOptions.Any())
            {
                overview.SelectedDate = overview.DayOptions.First().Date;
            }

            var allDienstens = _context.Dienstens
            .Include(d => d.Medewerker)
                .ThenInclude(m => m.Functie)
             .Include(d => d.Medewerker)
                .ThenInclude(m => m.Filiaal)
                    .ThenInclude(f => f.Afdelings)
            .ToList();

            overview.Diensten = allDienstens
                .Where(d => d.Datum == GetFirstDayOfWeek(year, weekNumber))
                .ToList();

            overview.Afdelingen = _context.Afdelingens.Include(f => f.Functies).ToList();
           
            overview.Medewerkers = _context.Medewerkers.ToList();

            return View(overview);
        }

        [HttpPost]
        public IActionResult RoosterOverview(RoosterOverviewViewModel model)
        {
            model.WeekNumber = model.WeekNumber;
            model.year = model.year;
            model.DayOptions = GetDayOptionsForWeek(model.year, model.WeekNumber);

            var allDienstens = _context.Dienstens
            .Include(d => d.Medewerker)
                .ThenInclude(m => m.Functie)
             .Include(d => d.Medewerker)
                .ThenInclude(m => m.Filiaal)
                    .ThenInclude(f => f.Afdelings)
            .ToList();

            model.Diensten = allDienstens
                .Where(d => d.Datum == model.SelectedDate)
                .ToList();

            model.Afdelingen = _context.Afdelingens.Include(f => f.Functies).ToList();
            
            model.Medewerkers = _context.Medewerkers.ToList();

            return View(model);
        }

        [HttpPost]
        public IActionResult UpdateWorkingHours(int dienstId, TimeSpan startTijd, TimeSpan endTijd)
        {
            var dienst = _context.Dienstens
                                 .Include(d => d.Beschikbaarheid)
                                 .FirstOrDefault(d => d.DienstenId == dienstId);
            var dienstStartTijd = dienst.StartTijd;
            var dienstEindTijd = dienst.EindTijd;

            if (dienst != null && dienst.Beschikbaarheid != null)
            {
                if (startTijd >= dienst.Beschikbaarheid.StartTijd && endTijd <= dienst.Beschikbaarheid.EindTijd && startTijd < endTijd)
                {
                    dienst.StartTijd = startTijd;
                    dienst.EindTijd = endTijd;
                    _context.Update(dienst);
                    _context.SaveChanges();
                }
                else
                {
                    var years = HttpContext.Session.GetInt32("SelectedYear") ?? default;
                    var weekNumbers = HttpContext.Session.GetInt32("SelectedWeekNumber") ?? default;
                    var selectedDates = DateTime.Parse(HttpContext.Session.GetString("SelectedDate") ?? DateTime.Now.ToString("o"));
                    var selectedDepartments = HttpContext.Session.GetInt32("SelectedDepartment") ?? default;

                    TempData["ErrorName"] = "UpdateWorkingHoursError";
                    TempData["ErrorMessage"] = "Uren moeten binnen beschikbaarheid zijn en/of starttijd kleiner dan eindtijd";

                    return RedirectToAction("RoosterCreate", new { year = years, weekNumber = weekNumbers, selectedDate = selectedDates, selectedDepartment = selectedDepartments });
                }
                
            }
            //var dienstId = _context.Dienstens.FirstOrDefault(d => d.BeschikbaarheidId == beschikbaarheidId);
            int selectedMedewerkerId = getMedewerkerFromBeschikbaarheidId(dienst.Beschikbaarheid.BeschikbaarheidId);
            CAOController.CheckAgeShiftRules(selectedMedewerkerId, dienstId);
            var DienstGotRemoved = _context.Dienstens.FirstOrDefault(d => d.DienstenId == dienstId);
            if (DienstGotRemoved == null)
            {
                AddToRoster(dienst.Beschikbaarheid.BeschikbaarheidId, dienstStartTijd, dienstEindTijd);
            }

            // Redirecting back to the RoosterCreate view with the necessary parameters
            int year = HttpContext.Session.GetInt32("SelectedYear") ?? default;
            int weekNumber = HttpContext.Session.GetInt32("SelectedWeekNumber") ?? default;
            DateTime selectedDate = DateTime.Parse(HttpContext.Session.GetString("SelectedDate") ?? DateTime.Now.ToString("o"));
            int selectedDepartment = HttpContext.Session.GetInt32("SelectedDepartment") ?? default;

            return RedirectToAction("RoosterCreate", new { year = year, weekNumber = weekNumber, selectedDate = selectedDate, selectedDepartment = selectedDepartment });
        }

        [HttpPost]
        public IActionResult DeleteFromRoster(int dienstId)
        {
            var dienst = _context.Dienstens
                                 .FirstOrDefault(d => d.DienstenId == dienstId);

            if (dienst != null)
            {
                _context.Dienstens.Remove(dienst);
                _context.SaveChanges();
            }

            int year = HttpContext.Session.GetInt32("SelectedYear") ?? DateTime.Now.Year;
            int weekNumber = HttpContext.Session.GetInt32("SelectedWeekNumber") ?? CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            DateTime selectedDate = DateTime.Parse(HttpContext.Session.GetString("SelectedDate") ?? DateTime.Now.ToString("o"));
            int selectedDepartment = HttpContext.Session.GetInt32("SelectedDepartment") ?? 1; 

            return RedirectToAction("RoosterCreate", new { year, weekNumber, SelectedDate = selectedDate.ToString("yyyy-MM-dd"), SelectedDepartment = selectedDepartment });
        }

        private int FetchPrognose(DateTime date, int departmentId)
        {
            var loggedInUser = GetLoggedInUser();
            if (loggedInUser == null || loggedInUser.FiliaalId == null)
            {
                return 0; // Or handle this scenario as needed
            }

            // Use FirstOrDefault to retrieve a single Prognose object
            Prognose? currentPrognose = _context.Prognoses
                                               .FirstOrDefault(p => p.Datum.Date == date.Date
                                                                    && p.FiliaalId == loggedInUser.FiliaalId
                                                                    && p.AfdelingId == departmentId);

            if (currentPrognose == null)
            {
                return 0; // Return 0 if no Prognose object was found
            }

            return currentPrognose.Uren; // Return the Uren property of the Prognose object
        }

        [HttpPost]
        public IActionResult ReturnWithError(string ErrorName, string ErrorMessage)
        {
            ModelState.AddModelError(ErrorName, ErrorMessage);

            // Prepare the ViewModel
            var years = HttpContext.Session.GetInt32("SelectedYear") ?? default;
            var weekNumbers = HttpContext.Session.GetInt32("SelectedWeekNumber") ?? default;
            var selectedDates = DateTime.Parse(HttpContext.Session.GetString("SelectedDate") ?? DateTime.Now.ToString("o"));
            var selectedDepartments = HttpContext.Session.GetInt32("SelectedDepartment") ?? default;

            TempData["ErrorName"] = ErrorName;
            TempData["ErrorMessage"] = ErrorMessage;

            return RedirectToAction("RoosterCreate", new { year = years, weekNumber = weekNumbers, selectedDate = selectedDates, selectedDepartment = selectedDepartments });
        }
    }


}
