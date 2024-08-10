using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeApplication.Data;
using RecipeApplication.Models;
using System.ComponentModel;
using System.Text;

namespace RecipeApplication.Controllers
{
    [Authorize]
    public class MealPlanController : Controller
    {
        private ApplicationDbContext _context;
        private UserManager<AppUser> _userManager;

        public MealPlanController(ApplicationDbContext context, UserManager<AppUser> usermanager) 
        {
            _context = context;
            _userManager = usermanager;
        }

        public IActionResult Index() => View(_context.MealPlans);

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create (CreateMealPlanVM mealPlanVM)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var userId = user?.Id;

                var mealPlan = new MealPlan
                {
                    Description = mealPlanVM.Description,
                    //UserId = User.Identity.Name,
                    UserId = userId,
                    StartDate = mealPlanVM.StartDate,
                    EndDate = mealPlanVM.EndDate,
                };
                
                _context.Add(mealPlan);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Something went wrong";
            return RedirectToAction(nameof(Create));
        }

        public async Task<IActionResult> Details(int id)
        {

            var mealPlan = await _context.MealPlans
            .Include(mp => mp.DailyMealPlans)
            .ThenInclude(dmp => dmp.MealPlanRecipes)
                .ThenInclude(mpr => mpr.Recipe)
             .FirstOrDefaultAsync(mp => mp.Id == id);
 

            if (mealPlan == null)
            {
                return NotFound();
            }

            //var mealPlanRecipes = await _context.MealPlanRecipes
            //    .Where(mpr => mpr.MealPlanId == id)
            //    .Include(mpr => mpr.Recipe)
            //    .ToListAsync();
            ViewBag.AvailableRecipes = await _context.Recipes.ToListAsync();
            return View(mealPlan);
        }


        public async Task <IActionResult> Delete(int id)
        {
            var mealPlan = await _context.MealPlans.FirstOrDefaultAsync(c => c.Id == id);
            if(mealPlan == null)
            {
                return NotFound();
            }

            _context.Remove(mealPlan);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AddRecipeToMealPlan(int mealPlanId, int recipeId, DateTime date, string mealType)
        {
            var stringDate = date.ToString();

            // Retrieve the meal plan along with related entities
            var mealPlan = await _context.MealPlans
                .Include(mp => mp.DailyMealPlans)
                    .ThenInclude(dmp => dmp.MealPlanRecipes)
                .FirstOrDefaultAsync(mp => mp.Id == mealPlanId);

            if (mealPlan == null)
            {
                return NotFound();
            }

            // Find or create the DailyMealPlan entry
            var dailyMealPlan = mealPlan.DailyMealPlans
                .FirstOrDefault(d => d.Date.Date == date.Date);

            if (dailyMealPlan == null)
            {
                dailyMealPlan = new DailyMealPlan
                {
                    Date = date,
                    MealPlanId = mealPlanId,
                    MealPlanRecipes = new List<MealPlanRecipe>()
                };

                _context.DailyMealPlans.Add(dailyMealPlan);
                await _context.SaveChangesAsync();
            }

            // Check if the recipe already exists for the given meal type
            var existingRecipe = dailyMealPlan.MealPlanRecipes
                .FirstOrDefault(r => r.RecipeId == recipeId && r.MealType == mealType);

            if (existingRecipe == null)
            {
                // Add the new recipe entry
                var mealPlanRecipe = new MealPlanRecipe
                {
                    RecipeId = recipeId,
                    MealPlanId = mealPlanId,
                    MealType = mealType,
                    PlannedDate = date
                };

                dailyMealPlan.MealPlanRecipes.Add(mealPlanRecipe);
                _context.MealPlanRecipes.Add(mealPlanRecipe);
            }

            await _context.SaveChangesAsync();


            return RedirectToAction("Details", new { id = mealPlanId });
        }

        private List<string> Ingredients(int mealPlanId)
        {
            var mealPlan =  _context.MealPlans
        .Include(mp => mp.DailyMealPlans)
        .ThenInclude(dmp => dmp.MealPlanRecipes)
        .ThenInclude(mpr => mpr.Recipe)
        .FirstOrDefault(mp => mp.Id == mealPlanId);

            if (mealPlan == null)
            {
                throw new Exception("Error");
            }

            var distinctIngredients = mealPlan.DailyMealPlans
                .SelectMany(dmp => dmp.MealPlanRecipes)
                .SelectMany(mpr => mpr.Recipe.Ingredients)
                .Distinct()
                .ToList();

            return distinctIngredients;
        }

        public IActionResult GenerateShoppingList(int mealPlanId)
        {
            var distinctIngredients = Ingredients(mealPlanId);

            // Pass the distinct ingredients to the view
            return PartialView("_ShoppingListModal", distinctIngredients);
        }

        public IActionResult DownloadList(int mealPlanId)
        {
            // Sample data to be written to the file
            //var content = "This is a sample file content";

            var content = string.Join("\n", Ingredients(mealPlanId));

            // Convert content to bytes
            var byteArray = Encoding.UTF8.GetBytes(content);

            // Create a memory stream to hold the file data
            using (var originalStream = new MemoryStream(byteArray))
            {
                // Copy to a new stream to avoid the "closed stream" issue
                var stream = new MemoryStream();
                originalStream.CopyTo(stream);
                stream.Position = 0; // Reset position to the beginning

                // Return the file as a downloadable file
                return File(stream, "text/plain", "ingredients.txt");
            }
        }

        [HttpGet("DownloadList/{name}")]
        public IActionResult DownloadList(string name)
        {
    

            var recipe = _context.Recipes.FirstOrDefault(c => c.Name == name);
            if (recipe == null) return NotFound();

            var content = string.Join("\n", recipe.Ingredients);

            // Convert content to bytes
            var byteArray = Encoding.UTF8.GetBytes(content);

            // Create a memory stream to hold the file data
            using (var originalStream = new MemoryStream(byteArray))
            {
                // Copy to a new stream
                var stream = new MemoryStream();
                originalStream.CopyTo(stream);
                stream.Position = 0; // Reset position to the beginning

                // Return the file as a downloadable file
                return File(stream, "text/plain", "ingredients.txt");
            }
        }



    }
}