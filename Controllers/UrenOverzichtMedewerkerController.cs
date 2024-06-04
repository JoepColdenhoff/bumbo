using Bumbo.Models;
using Bumbo.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Bumbo.Controllers
{
    public class UrenOverzichtMedewerkerController : Controller
    {
        private readonly BumboContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public UrenOverzichtMedewerkerController(BumboContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<ViewResult> Index(int Year, int weekNumber)
        {
            //week select & jaar setten
            if (Year == 0)
            {
                Year = DateTime.Now.Year;
                weekNumber = GetIso8601WeekOfYear(DateTime.Now);
            }

            if (weekNumber >= 53)
            {
                weekNumber = 1;
                Year++;
            }
            else if (weekNumber <= 0)
            {
                weekNumber = 52;
                Year--;
            }

            //medewerkerinfo/uren ophalen
            var currentUser = await _userManager.GetUserAsync(User);
            var medewerker = _context.Medewerkers
                .Include(m => m.Dienstens.Where(d => d.Datum.Date < DateTime.Today.Date))
                .Include(m => m.Filiaal)
                .FirstOrDefault(m => m.Email == currentUser.Email);

            var departmentId = medewerker.Dienstens
                .Where(d => d.Datum.Date < DateTime.Today.Date)
                .Select(d => d.AfdelingId)
                .FirstOrDefault();

            var departmentName = _context.Afdelingens
                .Where(a => a.AfdelingId == departmentId)
                .Select(a => a.Naam)
                .FirstOrDefault();

            var approvedHours = _context.Inklokkens
                .Where(i => i.Diensten.MedewerkerId == medewerker.MedewerkerId && i.Goedkeuring)
                .OrderBy(i => i.Start)
                .Select(i => new
                {
                    i.Start,
                    i.Eind
                })
                .ToList();

            // Set the viewModel.Year and viewModel.WeekNumber first
            var currentDate = DateTime.Today;
            var currentCulture = System.Globalization.CultureInfo.CurrentCulture;
            var calendar = currentCulture.Calendar;
            var calendarWeekRule = currentCulture.DateTimeFormat.CalendarWeekRule;
            var firstDayOfWeek = DayOfWeek.Monday;

            var viewModel = new UrenOverzichtMedewerkerViewModel
            {
                Year = Year,
                WeekNumber = weekNumber,
                previousWeek = GetPreviousWeekNumber(weekNumber, Year),
                nextWeek = GetNextWeekNumber(weekNumber, Year),
                FirstDayOfSelectedWeek = FirstDateOfWeekISO8601(Year, weekNumber),
                LastDayOfSelectedWeek = FirstDateOfWeekISO8601(Year, weekNumber).AddDays(6),
            };
            //check of uren goedgekeurd zijn
            if (approvedHours.Any())
            {
                viewModel.registeredStartTime = approvedHours.First().Start;
                viewModel.registeredEndTime = approvedHours.Last().Eind;
            }
            currentDate = viewModel.FirstDayOfSelectedWeek;
            var selectedWeekShifts = medewerker.Dienstens
                .Where(d => d.Datum.Date >= viewModel.FirstDayOfSelectedWeek && d.Datum.Date <= viewModel.LastDayOfSelectedWeek)
                .ToList();

            if (selectedWeekShifts.Count > 0)
            {
                viewModel.departmentName = departmentName;
                viewModel.diensten = selectedWeekShifts;

                //geplande uren en goedgekeurde uren markeren en setten
                foreach (var dienst in selectedWeekShifts)
                {
                    var plannedStartTime = dienst.StartTijd;
                    var plannedEndTime = dienst.EindTijd;
                    var dienstRegisteredStartTime = (viewModel.registeredStartTime ?? TimeSpan.Zero);
                    var dienstRegisteredEndTime = (viewModel.registeredEndTime ?? TimeSpan.Zero);
                    var plannedHours = (plannedEndTime - plannedStartTime).TotalMinutes;
                    var registeredHours = (dienstRegisteredEndTime - dienstRegisteredStartTime).TotalMinutes;
                    var textColor = registeredHours >= plannedHours ? "text-danger" : "text-success";
                    viewModel.textColors.Add(textColor);
                }
            }

            else
            {
                ModelState.AddModelError("NoShifts", "Er zijn nog geen geregistreerde gewerkte diensten voor deze week. :(.");
            }

            return View(viewModel);
        }

        public int GetPreviousWeekNumber(int currentWeekNumber, int currentYear)
        {
            DateTime currentDate = FirstDateOfWeekISO8601(currentYear, currentWeekNumber);
            DateTime previousWeekStart = currentDate.AddDays(-7);

            int previousYear = currentYear;
            int previousWeekNumber;

            if (currentWeekNumber == 1)
            {
                // If the current week is the first week, set the previous week to the last week of the previous year
                previousYear--;
                previousWeekNumber = GetIso8601WeekOfYear(new DateTime(previousYear, 12, 31));
            }
            else
            {
                previousWeekNumber = GetIso8601WeekOfYear(previousWeekStart);
            }

            return previousWeekNumber + (previousYear - currentYear) * 52;
        }

        public int GetNextWeekNumber(int currentWeekNumber, int currentYear)
        {
            DateTime currentDate = FirstDateOfWeekISO8601(currentYear, currentWeekNumber);
            DateTime nextWeekStart = currentDate.AddDays(7);

            int nextYear = currentYear;
            int nextWeekNumber;

            if (currentWeekNumber == GetIso8601WeekOfYear(new DateTime(currentYear, 12, 31)) && nextWeekStart.Year > currentYear)
            {
                // If the current week is the last week, set the next week to the first week of the next year
                nextYear++;
                nextWeekNumber = 1;
            }
            else
            {
                nextWeekNumber = GetIso8601WeekOfYear(nextWeekStart);
            }

            return nextWeekNumber + (nextYear - currentYear) * 52;
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


    }
}