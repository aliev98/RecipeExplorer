namespace RecipeApplication.Models
{
    public class DailyMealPlan
    {
        public int Id { get; set; }
        public int MealPlanId { get; set; }
        public MealPlan MealPlan { get; set; }
        public DateTime Date { get; set; }
        public ICollection<MealPlanRecipe> MealPlanRecipes { get; set; }
    }

}
