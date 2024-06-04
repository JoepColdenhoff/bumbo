using Microsoft.AspNetCore.Mvc;
using Bumbo.Models;
using OfficeOpenXml;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Bumbo.Controllers
{
    [Authorize(Roles = "Manager")]
    public class NormeringController : Controller
    {
        private readonly BumboContext _context;
        private List<string> eenheidLijst;
        private List<int> duurLijst;
        private List<string> normTemp;
        private List<string> activiteitNaam;
        private List<Normeringen> normeringenList;

        public NormeringController(BumboContext context)
        {
            _context = context;
            eenheidLijst = new List<string>();
            duurLijst = new List<int>();

            activiteitNaam = new List<string>();
            normTemp = new List<string>();
            normeringenList = new List<Normeringen>();
        }

        public IActionResult Normering()
        {
            var newestUploadDate = _context.Normeringens.Max(n => n.UploadDatum);

            var recenteNormeringen = _context.Normeringens
                .Where(n => n.UploadDatum == newestUploadDate)
                .Select(n => new Normeringen
                {
                    Eenheid = n.Eenheid,
                    Duur = n.Duur,
                    Activiteitens = n.Activiteitens
                })
                .ToList();

            var NormeringenViewModel = new NormeringenViewModel()
            {
                NormeringenList = recenteNormeringen
            };
            return View(NormeringenViewModel);
        }

        [HttpPost]
        public IActionResult LoadFile(IFormFile fileInput)
        {
            try
            {
                if (fileInput == null || fileInput.Length == 0)
                {
                    return RedirectToAction("Normering", "Normering");
                    /* return View("1");//ALLEEN VOOR CHECKEN NIET RELEVANT*/
                }

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var package = new ExcelPackage(fileInput.OpenReadStream()))
                {

                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();

                    if (worksheet == null)
                    {
                        return RedirectToAction("Normering", "Normering");
                        /*return View("2");//ALLEEN VOOR CHECKEN NIET RELEVANT*/
                    }

                    int rowCount = worksheet.Dimension.Rows;
                    int colCount = worksheet.Dimension.Columns;

                    activiteitNaam = new List<string>(Enumerable.Repeat("", rowCount - 1));
                    normTemp = new List<string>(Enumerable.Repeat("", rowCount - 1));

                    for (int row = 2; row <= rowCount; row++)
                    {
                        for (int col = 1; col <= colCount; col++)
                        {
                            var cellValue = worksheet.Cells[row, col].Value?.ToString() ?? "Error";

                            if (col == 1)
                            {
                                activiteitNaam[row - 2] = cellValue;
                            }
                            else
                            {
                                normTemp[row - 2] = cellValue;
                            }
                        }
                    }

                    try
                    {
                        TranslateNorm(normTemp, activiteitNaam);


                        return RedirectToAction("Normering", "Normering");
                    }
                    catch (Exception ex)//ALLEEN VOOR CHECKEN NIET RELEVANT
                    {
                        return RedirectToAction("Normering", "Normering");
                        /*return View("3");//ALLEEN VOOR CHECKEN NIET RELEVANT*/
                    }
                }
            }
            catch (Exception ex)//ALLEEN VOOR CHECKEN NIET RELEVANT
            {
                return RedirectToAction("Normering", "Normering");
                /*return View("4");//ALLEEN VOOR CHECKEN NIET RELEVANT*/
            }
        }

        private List<Normeringen> TranslateNorm(List<string> normTemp, List<string> activiteitNaam)
        {

            var tijd = DateTime.Now;
            for (int i = 0; i < normTemp.Count; i++)
            {
                string[] parts = normTemp[i].Split(' ');
                Normeringen normering = new Normeringen();
                if (i < activiteitNaam.Count)
                {
                    string currentActiviteitNaam = activiteitNaam[i];

                    List<string> searchWords = new List<string> { "minuten", "seconde", "klanten", "uur", "coli", "meter" };
                    List<int> intValues = parts.Select(part => ExtractIntFromPart(part)).Where(value => value != 0).ToList();
                    string eenheid = string.Join("/", searchWords.Where(word => parts.Contains(word)));

                    normering.Duur = intValues.LastOrDefault();
                    normering.UploadDatum = tijd;
                    normering.Eenheid = eenheid;

                    var matchingActiviteit = _context.Activiteitens.FirstOrDefault(a => a.Naam == currentActiviteitNaam);
                    if (matchingActiviteit != null)
                    {
                        normering.Activiteitens.Add(matchingActiviteit);
                    }

                    normeringenList.Add(normering);

                    _context.Normeringens.Add(normering);
                }
            }

            _context.SaveChanges();
            return normeringenList;
        }

        private int ExtractIntFromPart(string part)
        {
            if (int.TryParse(part, out int intValue))
            {
                return intValue;
            }
            return 0;
        }

        public IActionResult DeleteNormering(DateTime uploadDatum)
        {
            try
            {
                /*var normeringenToRemove = _context.Normeringens
                .Where(n => n.UploadDatum == uploadDatum)
                .Join(_context.Activiteitens,
                 normering => normering.ActiviteitId,
                    activiteit => activiteit.ActiviteitId,
                    (normering, activiteit) => new Normeringen
                    {
                        Activiteit = activiteit,
                        Eenheid = normering.Eenheid,
                        Duur = normering.Duur
                    })
                    .ToList();*/
                var normeringenToRemove = _context.Normeringens
                    .Include(n => n.Activiteitens) // This will include the related Activiteiten
                    .Where(n => n.UploadDatum == uploadDatum)
                    .ToList();


                _context.Normeringens.RemoveRange(normeringenToRemove);
                _context.SaveChanges();

                TempData["Message"] = "Normering records have been successfully deleted.";
            }
            catch (Exception ex)
            {
                // Handle exceptions and set an error message if needed.
                TempData["ErrorMessage"] = "An error occurred while deleting normering records.";
            }

            return RedirectToAction("Normering"); // Redirect to the index or a relevant page.
        }
    }
}
