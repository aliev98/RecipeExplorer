using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RecipeApplication.Data;
using RecipeApplication.Models;
using System.Diagnostics;
using System.Dynamic;
using System.Security.Claims;

namespace RecipeApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly UserManager<AppUser> _userManager;

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory, ApplicationDbContext context, UserManager<AppUser> usermanager)
        {
            _logger = logger;
            _context = context;
            _userManager = usermanager;
            _httpClientFactory = httpClientFactory;
        }

        public async Task <IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.GetUserAsync(User);
            ViewBag.User = user;

            var favorites = _context.FavoriteRecipes.Where(f => f.UserId == userId).Include(c=>c.Recipe).ToList();
            return View(favorites);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
