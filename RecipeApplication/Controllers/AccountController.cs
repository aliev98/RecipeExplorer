using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Core.Types;
using RecipeApplication.Data;
using RecipeApplication.Models;
using System.Diagnostics.Eventing.Reader;
using System.Security.Claims;

namespace RecipeApplication.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        //public IActionResult Index()
        //{
        //    return View();
        //}

        public IActionResult Login() => View();
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM vm)
        {
            if (ModelState.IsValid)
            {
                // Get the user's current identity
                //var user = await _userManager.FindByEmailAsync(vm.Email);

                //var claims = new List<Claim>
                //{
                //    new Claim("FirstName", user.FirstName),
                //    new Claim("LastName", user.LastName)
                //};

               // var identity = new ClaimsIdentity(claims);

                // Sign the user in again with the new claims
          //      await _signInManager.SignInWithClaimsAsync(user, vm.RememberMe, claims);


                var result = await _signInManager.PasswordSignInAsync(vm.Email, vm.Password, false, false);
                
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Recipe");
                }

                ViewBag.Error = "Incorrect email or password";

                ModelState.AddModelError("", "Invalid login attempt.");
            }

            return View(vm);
        }


        [HttpPost]
        public async Task <IActionResult> Register(RegisterVM vm)
        {
            if (ModelState.IsValid)
            {
                var user = new AppUser
                {
                    UserName = vm.Email,
                    Email = vm.Email,
                    FirstName = vm.FirstName,
                    LastName = vm.LastName
                };

                var result = await _userManager.CreateAsync(user, vm.Password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Recipe");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                ViewBag.Error = "Something went wrong";
            }

            return View(vm);
        }

        //[HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
