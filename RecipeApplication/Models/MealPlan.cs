using System.ComponentModel.DataAnnotations;

namespace RecipeApplication.Models
{
    public class MealPlan
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string UserId { get; set; }
        public AppUser User { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }
        public ICollection<DailyMealPlan> DailyMealPlans { get; set; } = new List<DailyMealPlan>();
        public ICollection<MealPlanRecipe> MealPlanRecipes { get; set; } // Ensure this matches your actual entity names
    }
}