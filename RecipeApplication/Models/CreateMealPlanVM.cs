using System.ComponentModel.DataAnnotations;

namespace RecipeApplication.Models
{
    public class CreateMealPlanVM
    {
        [Required]
        public string Description { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
    }
}
