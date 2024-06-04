using Bumbo.Models;
using Bumbo.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Bumbo.Controllers
{
    [Authorize(Roles = "Manager")]
    public class FilialenController : Controller
    {
        private readonly BumboContext _context;

        public FilialenController(BumboContext context)
        {
            _context = context;
        }

        public Medewerker GetLoggedInUser()
        {
            var userEmail = User.Identity.Name;

            // Assuming there's a field in Medewerker that links to the IdentityUser's ID
            var medewerker = _context.Medewerkers.FirstOrDefault(m => m.Email == userEmail);

            return medewerker;
        }

        public IActionResult Index()
        {
            int? filiaalId = GetLoggedInUser()?.FiliaalId;

            var currentFiliaalLocatie = _context.Filialens
                .Where(a => a.FiliaalId == filiaalId)
                .Select(a => a.Locatie)
                .FirstOrDefault();

            var filialenNearby = _context.Filialens
                .ToList() // Fetch all first, since we need to transform each location
                .Where(a => a.FiliaalId != filiaalId)
                .Take(10)
                .ToList();

            // Bereken afstanden in de controller
            var filialenWithDistance = new List<FiliaalDistanceViewModel>();
            foreach (var filiaal in filialenNearby)
            {
                //double distance = CalculateDistance(huidigFiliaalLocatie, filiaal.Locatie);

                var distanceDegrees = currentFiliaalLocatie.Distance(filiaal.Locatie);
                var distanceKilometers = DegreesToKilometers(distanceDegrees);

                var filiaalWithDistance = new FiliaalDistanceViewModel
                {
                    Filiaal = filiaal,
                    Distance = distanceKilometers
                };

                filialenWithDistance.Add(filiaalWithDistance);
            }

            filialenWithDistance = filialenWithDistance.OrderBy(f => f.Distance).ToList();
            return View(filialenWithDistance);
        }

        double DegreesToKilometers(double degrees)
        {
            // Average radius of the Earth in kilometers
            const double EarthRadius = 6371.0;
            return degrees * (Math.PI / 180) * EarthRadius;
        }

    }
}