namespace RecipeApi.Dtos;

public record MealSummaryDto(int Id, string Name, string Thumbnail);

public record IngredientDto(string Name, string Measure);

public record MealDto(
    int Id,
    string Name,
    string Thumbnail,
    string Category,
    string Area,
    string Instructions,
    string YoutubeUrl,
    List<IngredientDto> Ingredients);

public record CategoryDto(int Id, string Name, string Thumbnail, string Description);