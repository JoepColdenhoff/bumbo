using Bumbo.Models;
using Bumbo.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bumbo.Controllers
{
    [Authorize(Roles = "Manager")]
    public class MedewerkerController : Controller
    {
        private readonly BumboContext _context;

        public MedewerkerController(BumboContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(MedewerkerViewModel viewModel)
        {
            viewModel.Afdelingen = _context.Afdelingens.ToList();
            viewModel.Afdeling = "alle afdelingen";

            viewModel.medewerkers = await _context.Medewerkers
                .Include(m => m.Filiaal)
                .Include(m => m.Functie)
                   .Where(m => m.Verwijdert == false
                && m.FiliaalId == GetLoggedInUser().FiliaalId
                && m.MedewerkerId != GetLoggedInUser().MedewerkerId)
                .OrderBy(m => m.Functie)
                .ThenBy(m => m.Achternaam)
                .ThenBy(m => m.Voornaam)
                .ToListAsync();

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Index(int afdelingId, MedewerkerViewModel viewModel)
        {
            viewModel.Afdelingen = _context.Afdelingens.ToList();
            viewModel.Afdeling = _context.Afdelingens.Where(a => a.AfdelingId == afdelingId).FirstOrDefault().Naam;
            viewModel.medewerkers = await _context.Medewerkers
                .Include(m => m.Filiaal)
                .Include(m => m.Functie)
                   .Where(m => m.Verwijdert == false
                && m.FiliaalId == GetLoggedInUser().FiliaalId
                && m.MedewerkerId != GetLoggedInUser().MedewerkerId
                && m.Functie.Afdelings.Any(afdeling => afdeling.AfdelingId == afdelingId))
                .OrderBy(m => m.Functie)
                .ThenBy(m => m.Achternaam)
                .ThenBy(m => m.Voornaam)
                .ToListAsync();

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Info(int? id, MedewerkerInfoViewModel viewModel)
        {
            if (id == null || _context.Medewerkers == null)
            {
                return NotFound();
            }

            var medewerker = await _context.Medewerkers
                .Include(m => m.Filiaal)
                .Include(m => m.Functie)
                .FirstOrDefaultAsync(m => m.MedewerkerId == id);

            if (medewerker == null)
            {
                return NotFound();
            }

            var afdelingen = await _context.Afdelingens
                .Where(a => a.Filiaals.Any(f => f.FiliaalId == medewerker.FiliaalId))
                .ToListAsync();

            viewModel.MedewerkerId = medewerker.MedewerkerId;
            viewModel.Voornaam = medewerker.Voornaam;
            viewModel.Tussenvoegsel = medewerker.Tussenvoegsel;
            viewModel.Achternaam = medewerker.Achternaam;
            viewModel.Filiaal = medewerker.Filiaal;
            viewModel.Plaats = medewerker.Plaats;
            viewModel.Straatnaam = medewerker.Straatnaam;
            viewModel.Postcode = medewerker.Postcode;
            viewModel.Huisnummer = medewerker.Huisnummer;
            viewModel.Geboortedatum = medewerker.Geboortedatum.ToShortDateString();
            viewModel.Leeftijd = BerekenLeeftijd(medewerker.Geboortedatum);
            viewModel.Email = medewerker.Email;
            viewModel.Telefoonnummer = medewerker.Telefoonnummer;
            viewModel.Functie = medewerker.Functie;
            viewModel.Indienst = medewerker.Indienst.ToShortDateString();
            viewModel.Afdelingen = afdelingen;

            return View(viewModel);
        }
        private int BerekenLeeftijd(DateTime geboortedatum)
        {
            DateTime huidigeDatum = DateTime.Now;
            int leeftijd = huidigeDatum.Year - geboortedatum.Year;

            if (geboortedatum.Date > huidigeDatum.AddYears(-leeftijd))
            {
                leeftijd--;
            }

            return leeftijd;
        }

        [HttpGet]
        public IActionResult MedewerkerCreate()
        {
            var viewModel = new MedewerkerFormViewModel
            {
                Functies = _context.Functies.ToList()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MedewerkerCreate(MedewerkerFormViewModel viewModel)
        {
            var newMedewerker = new Medewerker
            {
                FunctieId = viewModel.FunctieId,
                FiliaalId = GetLoggedInUser().FiliaalId,
                Voornaam = viewModel.Voornaam,
                Tussenvoegsel = viewModel.Tussenvoegsel,
                Achternaam = viewModel.Achternaam,
                Email = viewModel.Email,
                Telefoonnummer = viewModel.Telefoonnummer,
                Geboortedatum = viewModel.Geboortedatum,
                Postcode = viewModel.Postcode,
                Huisnummer = viewModel.Huisnummer,
                Straatnaam = viewModel.Straatnaam,
                Plaats = viewModel.Plaats,
                Indienst = viewModel.Indienst,
                Verwijdert = false
            };

            _context.Medewerkers.Add(newMedewerker);
            await _context.SaveChangesAsync();


            var functieName = await _context.Functies.Where(f => f.FunctieId == viewModel.FunctieId).FirstOrDefaultAsync();

            return RedirectToAction("Register", "Authentication", new { email = newMedewerker.Email, functie = functieName.Naam});
        }

        [HttpGet]
        public async Task<IActionResult> MedewerkerUpdate(int? id, MedewerkerFormViewModel viewModel)
        {
            ModelState.Clear();

            if (id == null || _context.Medewerkers == null)
            {
                return NotFound();
            }

            var medewerker = await _context.Medewerkers
                .Include(m => m.Filiaal)
                .Include(m => m.Functie)
                .FirstOrDefaultAsync(m => m.MedewerkerId == id);

            if (medewerker == null)
            {
                return NotFound();
            }

            viewModel.MedewerkerId = medewerker.MedewerkerId;
            viewModel.FunctieId = medewerker.FunctieId;
            viewModel.FiliaalId = medewerker.FiliaalId;
            viewModel.Voornaam = medewerker.Voornaam;
            viewModel.Tussenvoegsel = medewerker.Tussenvoegsel;
            viewModel.Achternaam = medewerker.Achternaam;
            viewModel.Telefoonnummer = medewerker.Telefoonnummer;
            viewModel.Geboortedatum = medewerker.Geboortedatum;
            viewModel.Postcode = medewerker.Postcode;
            viewModel.Huisnummer = medewerker.Huisnummer;
            viewModel.Straatnaam = medewerker.Straatnaam;
            viewModel.Plaats = medewerker.Plaats;
            viewModel.Indienst = medewerker.Indienst;
            viewModel.Functies = _context.Functies.ToList();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MedewerkerUpdate(MedewerkerFormViewModel viewModel)
        {

            var medewerkerToUpdate = await _context.Medewerkers.FindAsync(viewModel.MedewerkerId);

            if (medewerkerToUpdate == null)
            {
                return NotFound();
            }

            medewerkerToUpdate.FunctieId = viewModel.FunctieId;
            medewerkerToUpdate.Voornaam = viewModel.Voornaam;
            medewerkerToUpdate.Tussenvoegsel = viewModel.Tussenvoegsel;
            medewerkerToUpdate.Achternaam = viewModel.Achternaam;
            medewerkerToUpdate.Telefoonnummer = viewModel.Telefoonnummer;
            medewerkerToUpdate.Geboortedatum = viewModel.Geboortedatum;
            medewerkerToUpdate.Postcode = viewModel.Postcode;
            medewerkerToUpdate.Huisnummer = viewModel.Huisnummer;
            medewerkerToUpdate.Straatnaam = viewModel.Straatnaam;
            medewerkerToUpdate.Plaats = viewModel.Plaats;
            medewerkerToUpdate.Indienst = viewModel.Indienst;

            _context.Update(medewerkerToUpdate);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"{medewerkerToUpdate.Voornaam}{ medewerkerToUpdate.Tussenvoegsel} {medewerkerToUpdate.Achternaam} is succesvol veranderd";
            
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> MedewerkerDelete(int? id)
        {
            if (id == null || _context.Medewerkers == null)
            {
                return NotFound();
            }

            var medewerker = await _context.Medewerkers
                .Include(m => m.Filiaal)
                .Include(m => m.Functie)
                .Include(m => m.Dienstens)
                .FirstOrDefaultAsync(m => m.MedewerkerId == id);

            if (medewerker == null)
            {
                return NotFound();
            }

            if (medewerker.Dienstens != null && medewerker.Dienstens.Any())
            {
                DateTime maxDienstDate = medewerker.Dienstens.Max(d => d.Datum);

                if (maxDienstDate >= DateTime.Today)
                {
                    TempData["ErrorMessage"] = "Kan medewerker niet verwijderen omdat er diensten zijn gepland voor vandaag of na vandaag.";
                    return RedirectToAction("Info", new { id = id });
                }
            }


            medewerker.Verwijdert = true;
            await _context.SaveChangesAsync();

            TempData["Message"] = $"{medewerker.Voornaam}{ medewerker.Tussenvoegsel} {medewerker.Achternaam} is succesvol verwijderd";
            return RedirectToAction("Index");
        }

        private Medewerker GetLoggedInUser()
        {
            var userEmail = User.Identity.Name;

            var medewerker = _context.Medewerkers.FirstOrDefault(m => m.Email == userEmail);

            if (medewerker == null)
            {
                Console.WriteLine("Medewerker not found");
            }

            return medewerker;
        }
    }
}
