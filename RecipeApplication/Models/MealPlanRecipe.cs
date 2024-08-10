namespace RecipeApplication.Models
{
    public class MealPlanRecipe
    {
        public int Id { get; set; }
        public int DailyMealPlanId { get; set; }
        public DailyMealPlan DailyMealPlan { get; set; }
        public int MealPlanId { get; set; }
        public MealPlan MealPlan { get; set; }
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }
        public DateTime PlannedDate { get; set; }
        public string MealType { get; set; } // Breakfast, Lunch, Dinner, etc.
    }
}