using Bumbo.Models;
using Bumbo.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bumbo.Controllers
{
    public class KlokkenController : Controller
    {
        private BumboContext _context;
        private UserManager<IdentityUser> _userManager;

        public KlokkenController(BumboContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [HttpGet]
        public async Task<IActionResult> IndexAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var medewerker = _context.Medewerkers
                .Include(m => m.Dienstens)
                .ThenInclude(d => d.Inklokken)
                .FirstOrDefault(m => m.Email == currentUser.Email);

            var viewModel = new InklokkenViewModel();

            if (medewerker != null)
            {
                //checken of er al een dienst is
                viewModel.IsKlokt = medewerker.Dienstens.Any(d => d.Inklokken.Any(i => i.Eind == null && i.Start != null));

                if (viewModel.IsKlokt)
                {
                    var shift = medewerker.Dienstens.FirstOrDefault(d => d.Inklokken.Any(i => i.Eind == null && i.Start != null));
                    var clockedInRecord = shift.Inklokken.First(i => i.Eind == null && i.Start != null);
                    viewModel.ClockedIn = DateTime.Today.Add(clockedInRecord.Start);
                }
            }
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> HandleInklokken(InklokkenViewModel viewModel)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var medewerker = _context.Medewerkers
                .Include(m => m.Dienstens.Where(d => d.Datum.Date == DateTime.Today.Date)) //meteen betreffende dienst van huidige dag ophalen
                .FirstOrDefault(m => m.Email == currentUser.Email);

            if (medewerker != null)
            {
                var shift = medewerker.Dienstens.FirstOrDefault(d => d.Datum.Date == DateTime.Today.Date);

                if (shift != null)
                {
                    viewModel.IsKlokt = true;
                    viewModel.ClockedIn = DateTime.Now;
                    viewModel.TimerMessage = "Je bent succesvol ingeklokt, geniet van je werkdag! :)";

                    var inklokkenRecord = new Inklokken
                    {
                        DienstenId = shift.DienstenId,
                        Start = DateTime.Now.TimeOfDay,
                        Eind = null,
                        Goedkeuring = false
                    };

                    _context.Inklokkens.Add(inklokkenRecord);
                    _context.SaveChanges();
                }
                else
                {
                    viewModel.ErrorMessage = "Je hebt nu geen geplande diensten staan in het systeem waarvoor je je kunt inklokken :(";
                    return View("Index", viewModel);
                }
            }

            return View("Index", viewModel);
        }




        [HttpPost]
        public async Task<IActionResult> HandleUitklokken(InklokkenViewModel viewModel)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var medewerker = _context.Medewerkers
                .Include(m => m.Dienstens)
                .ThenInclude(d => d.Inklokken)
                .FirstOrDefault(m => m.Email == currentUser.Email);

            if (medewerker != null)
            {
                var Clocked = medewerker.Dienstens
                    .SelectMany(d => d.Inklokken)
                    .FirstOrDefault(i => i.Eind == null);

                if (Clocked != null)
                {
                    Clocked.Eind = DateTime.Now.TimeOfDay;
                    _context.SaveChanges();

                    viewModel.ClockedOut = DateTime.Now;

                    viewModel.TimerMessage = "Je bent succesvol uitgeklokt, tot ziens! :)";
                }
                viewModel.IsKlokt = medewerker.Dienstens.Any(d => d.Inklokken.Any(i => i.Eind == null));
            }

            return View("Index", viewModel);
        }

    }
}
