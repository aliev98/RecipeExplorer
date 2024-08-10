using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using RecipeApplication.Models;
using System.Reflection.Emit;

namespace RecipeApplication.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<MealPlan> MealPlans { get; set; }
        public DbSet<MealPlanRecipe> MealPlanRecipes { get; set; }
        public DbSet<DailyMealPlan> DailyMealPlans { get; set; }
        public DbSet<FavoriteRecipe> FavoriteRecipes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure MealPlan and its relationship with DailyMealPlan
            builder.Entity<MealPlan>()
                .HasMany(mp => mp.DailyMealPlans)
                .WithOne(dmp => dmp.MealPlan)
                .HasForeignKey(dmp => dmp.MealPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure DailyMealPlan and its relationship with MealPlanRecipe
            builder.Entity<DailyMealPlan>()
                .HasMany(dmp => dmp.MealPlanRecipes)
                .WithOne(mpr => mpr.DailyMealPlan)
                .HasForeignKey(mpr => mpr.DailyMealPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure MealPlanRecipe with MealPlan
            builder.Entity<MealPlanRecipe>()
                .HasOne(mpr => mpr.MealPlan)
                .WithMany(mp => mp.MealPlanRecipes)
                .HasForeignKey(mpr => mpr.MealPlanId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure MealPlanRecipe with Recipe
            builder.Entity<MealPlanRecipe>()
                .HasOne(mpr => mpr.Recipe)
                .WithMany() // No navigation property in Recipe
                .HasForeignKey(mpr => mpr.RecipeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Many-to-many relationship between User and FavoriteRecipe

            builder.Entity<FavoriteRecipe>()
                .HasKey(fr => new { fr.UserId, fr.RecipeId });

            builder.Entity<FavoriteRecipe>()
                .HasOne(fr => fr.User)
                .WithMany()
                .HasForeignKey(fr => fr.UserId);

            builder.Entity<FavoriteRecipe>()
                .HasOne(fr => fr.Recipe)
                .WithMany()
                .HasForeignKey(fr => fr.RecipeId);
        }

        public void ClearTable()
        {
            Database.ExecuteSqlRaw("DELETE FROM " +
                "MealPlanRecipes");
            Database.ExecuteSqlRaw("DELETE FROM " +
                "Recipes");
            Database.ExecuteSqlRaw("DELETE FROM " +
                "MealPlans");
            Database.ExecuteSqlRaw("DELETE FROM " +
                "DailyMealPlans");

            Database.ExecuteSqlRaw("DBCC CHECKIDENT ('Recipes', RESEED, 0);");
            Database.ExecuteSqlRaw("DBCC CHECKIDENT ('MealPlans', RESEED, 0);");
            Database.ExecuteSqlRaw("DBCC CHECKIDENT ('MealPlanRecipes', RESEED, 0);");
            Database.ExecuteSqlRaw("DBCC CHECKIDENT ('DailyMealPlans', RESEED, 0);");
        }

    }
}
