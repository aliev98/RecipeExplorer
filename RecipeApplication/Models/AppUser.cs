using Microsoft.AspNetCore.Identity;

namespace RecipeApplication.Models
{
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        //public ICollection<UserRecipe> FavoriteRecipes { get; set; } = new List<UserRecipe>();
    }
}