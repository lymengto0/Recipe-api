namespace RecipeApi.Models;

public class Meal
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Thumbnail { get; set; } = "";
    public string Area { get; set; } = "";
    public string Instructions { get; set; } = "";
    public string YoutubeUrl { get; set; } = "";

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public List<Ingredient> Ingredients { get; set; } = new();
}