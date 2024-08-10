using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RecipeApplication.Data;
using RecipeApplication.Models;
using System.Diagnostics.Metrics;
using System.Dynamic;
using System.Net.Http;
using static RecipeApplication.Controllers.RecipeController;

namespace RecipeApplication.Controllers
{
    public class RecipeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        public RecipeController(IHttpClientFactory httpClientFactory, ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _httpClientFactory = httpClientFactory;
            _context = context;
            _userManager = userManager;
            //_context.ClearTable();
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (_context.Recipes.Count() == 0)
            {
                // Fetch recipes from API
                var recipes = new List<Recipe>();

                var client = _httpClientFactory.CreateClient();

                for (char letter = 'a'; letter <= 'z'; letter++)
                {
                    var response = await client.GetStringAsync($"https://www.themealdb.com/api/json/v1/1/search.php?f={letter}");
                    var Iresult = JsonConvert.DeserializeObject<JToken>(response);
                    var t = Iresult["meals"] as JArray;
                    
                    if (t == null) continue;

                    foreach (var mealToken in t)
                    {
                        var ingredients = new List<string>();
                        var measurements = new List<string>();

                        for (int i = 1; i <= 20; i++)
                        {
                            var ingredient = mealToken[$"strIngredient{i}"]?.ToString();
                            var measurement = mealToken[$"strMeasure{i}"]?.ToString();

                            if (string.IsNullOrWhiteSpace(ingredient))
                            {
                                break;
                            }

                            // Add only if the ingredient is not empty
                            ingredients.Add(ingredient);
                            measurements.Add(measurement);
                        }

                        var recipe = new Recipe
                        {
                            MealId = mealToken["idMeal"]?.ToString() ?? string.Empty,
                            Name = mealToken["strMeal"]?.ToString() ?? string.Empty,
                            Category = mealToken["strCategory"]?.ToString() ?? string.Empty,
                            Instructions = mealToken["strInstructions"]?.ToString() ?? string.Empty,
                            Area = mealToken["strArea"]?.ToString() ?? string.Empty,
                            ImageUrl = mealToken["strMealThumb"]?.ToString() ?? string.Empty,
                            YoutubeUrl = mealToken["strYoutube"]?.ToString() ?? string.Empty,
                            Ingredients = ingredients,
                            Measurements = measurements
                        };

                        recipes.Add(recipe);
                    }
                }

                _context.Recipes.AddRange(recipes);
                await _context.SaveChangesAsync();
            }

            var allRecipes = await _context.Recipes.ToListAsync();

            return View(allRecipes);
        }

        public async Task<IActionResult> Index(string searchTerm)
        {
            var recipes = _context.Recipes.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                recipes = recipes.Where(r => r.Name.Contains(searchTerm));
            }

            var recipeList = await recipes.ToListAsync();
            return PartialView("_RecipeList", recipeList); // Return a partial view with the list
        }

        public async Task<IActionResult> Details(string name)
        {


            if (string.IsNullOrEmpty(name))
            {
                return BadRequest("Invalid name");
            }

            var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.Name == name);

            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                 ViewBag.MealPlans = _context.MealPlans.Where(c=>c.UserId==user.Id).ToList();

            }
            
            return View(recipe);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFavorite(string name)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.Name == name);

            if (recipe == null)
            {
                return NotFound();
            }

            var existingFavorite = await _context.FavoriteRecipes.FirstOrDefaultAsync(fr => fr.UserId == user.Id && fr.RecipeId == recipe.Id);

            if (existingFavorite == null)
            {
                var favoriteRecipe = new FavoriteRecipe
                {
                    UserId = user.Id,
                    RecipeId = recipe.Id
                };

                _context.FavoriteRecipes.Add(favoriteRecipe);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Recipe added to favorites";
            TempData["Scroll"] = "#MyAnchor";

            return RedirectToAction("Details", new { name });
        }
        public async Task<IActionResult> RemoveFavorite(string name)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.Name == name);

            var fav = await _context.FavoriteRecipes.FirstOrDefaultAsync(c => c.UserId == user.Id && c.Recipe.Name == recipe.Name);

            _context.Remove(fav);

            _context.SaveChanges();

            return RedirectToAction("Index","Home");
        }



        public class RecipeResponse
        {
            [JsonProperty("meals")]
            public List<Meal> Meals { get; set; }
        }

        public class Meal
        {
            [JsonProperty("idMeal")]
            public string IdMeal { get; set; }

            [JsonProperty("strMeal")]
            public string StrMeal { get; set; }

            [JsonProperty("strCategory")]
            public string StrCategory { get; set; }

            [JsonProperty("strInstructions")]
            public string StrInstructions { get; set; }

            [JsonProperty("strArea")]
            public string StrArea { get; set; }

            [JsonProperty("strMealThumb")]
            public string StrMealThumb { get; set; }

            [JsonProperty("strYoutube")]
            public string StrYoutube { get; set; }
        }

    }
}