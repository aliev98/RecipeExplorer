namespace RecipeApplication.Models
{
    public class RecipeDetailsVM
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public string Instructions { get; set; }
        public string Area { get; set; }
        public string ImageUrl { get; set; }
        public string YoutubeUrl { get; set; }
        public List<string> Ingredients { get; set; }
        public List<string> Measurements { get; set; }
    }
}
