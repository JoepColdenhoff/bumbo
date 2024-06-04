
using Bumbo.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;

namespace Bumbo.Controllers { 

    public class DienstenController : Controller
    {
    private readonly BumboContext _context;

    public DienstenController(BumboContext context)
    {
        _context = context;
    }

        public Medewerker GetLoggedInUser()
        {
            var userEmail = User.Identity.Name;

         
            var medewerker = _context.Medewerkers.FirstOrDefault(m => m.Email == userEmail);

            return medewerker;
        }

        public async Task<IActionResult> Index(int medewerkerId = 1, int weekOffset = 0)
        {
            var loggedInUser = GetLoggedInUser();
            int? filiaalId = loggedInUser?.FiliaalId;

            var currentDate = DateTime.Today.AddDays(weekOffset * 7);
            var firstDayOfWeek = GetFirstDayOfWeekMonday(currentDate);
            var weekDates = Enumerable.Range(0, 7).Select(i => firstDayOfWeek.AddDays(i)).ToList();

            var weekSchedule = new Dictionary<DateTime, List<Diensten>>();

            foreach (var date in weekDates)
            {
                var diensten = await _context.Dienstens
                    .Include(d => d.Medewerker)
                        .ThenInclude(m => m.Functie) 
                    .Where(d => d.Datum.Date == date.Date && d.Medewerker.FiliaalId == filiaalId)
                    .ToListAsync();

                weekSchedule[date] = diensten;
            }

            ViewData["WeekSchedule"] = weekSchedule;
            ViewData["MedewerkerId"] = medewerkerId;
            ViewData["WeekOffset"] = weekOffset;

            return View();
        }
        
        
        private DateTime GetFirstDayOfWeekMonday(DateTime date)
    {
      
        int delta = DayOfWeek.Monday - date.DayOfWeek;
        if (delta > 0) delta -= 7;
        return date.AddDays(delta);
    }

}


}
