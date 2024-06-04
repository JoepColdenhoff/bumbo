using Bumbo.Models;
using Bumbo.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Bumbo.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly BumboContext _context;

        public AuthenticationController(BumboContext context, SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {

            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return View("Logout"); // Return the Logout view
        }

        [Authorize(Roles = "Manager")]
        [HttpGet]
        public IActionResult Register(String email)
        {
            Account model = new Account();
            model.Email = email;
            return View(model);
        }

        [Authorize(Roles = "Manager")]
        [HttpPost]
        public async Task<IActionResult> Register(Account model)
        {
           
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = model.Email, Email = model.Email };

                // Use the UserManager to create a new user
                var result = await _userManager.CreateAsync(user, model.Wachtwoord);

                if (model.functie != "Afdelinghoofd")
                {
                    await _userManager.AddToRoleAsync(user, "Medewerker");
                } 
                else if(model.functie == "Afdelinghoofd")
                {
                    await _userManager.AddToRoleAsync(user, "Manager");
                }

                if (result.Succeeded)
                {
                    TempData["Message"] = "Medewerker is succesvol toegevoegd";

                    //await _signInManager.SignInAsync(user, isPersistent: false);

                    return RedirectToAction("Index", "Medewerker");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            return View(model);
        }




        [HttpPost]
        public async Task<IActionResult> Login(Account model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Wachtwoord, false, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    var roles = await _userManager.GetRolesAsync(user);

                    if (roles.Contains("Manager"))
                    {
                        int currentYear = DateTime.Now.Year;
                        return RedirectToAction("index", "Prognose", new RouteValueDictionary(new { year = currentYear }));
                    }
                    else if (roles.Contains("Medewerker"))
                    {
                        return RedirectToAction("index", "BeschikbaarheidMedewerker");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Ongeldige gebruikersnaam of wachtwoord");
                }
            }

            return View();
        }

    }

}

