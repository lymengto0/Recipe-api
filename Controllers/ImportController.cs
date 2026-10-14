using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeApi.Data;
using RecipeApi.Models;

namespace RecipeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImportController(AppDbContext db, IHttpClientFactory factory, IWebHostEnvironment env)
    : ControllerBase
{
    private const string BaseUrl = "https://www.themealdb.com/api/json/v1/1";

    private static string S(JsonElement e, string name) =>
        e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String
            ? v.GetString() ?? ""
            : "";

    [HttpPost]
    public async Task<IActionResult> Run([FromQuery] int perCategory = 8)
    {
        if (!env.IsDevelopment()) return NotFound();             // never available in production
        if (await db.Meals.AnyAsync()) return BadRequest("Already imported.");

        var http = factory.CreateClient();

        // 1. Categories
        var catJson = await http.GetFromJsonAsync<JsonElement>($"{BaseUrl}/categories.php");
        var categories = new List<Category>();
        foreach (var c in catJson.GetProperty("categories").EnumerateArray())
        {
            categories.Add(new Category
            {
                Name = S(c, "strCategory"),
                Thumbnail = S(c, "strCategoryThumb"),
                Description = S(c, "strCategoryDescription"),
            });
        }
        db.Categories.AddRange(categories);
        await db.SaveChangesAsync();   // now each category has an Id

        // 2. Meals for each category
        foreach (var cat in categories)
        {
            var list = await http.GetFromJsonAsync<JsonElement>(
                $"{BaseUrl}/filter.php?c={Uri.EscapeDataString(cat.Name)}");

            if (list.GetProperty("meals").ValueKind != JsonValueKind.Array) continue;

            foreach (var item in list.GetProperty("meals").EnumerateArray().Take(perCategory))
            {
                var detail = await http.GetFromJsonAsync<JsonElement>(
                    $"{BaseUrl}/lookup.php?i={S(item, "idMeal")}");
                var m = detail.GetProperty("meals")[0];

                var meal = new Meal
                {
                    Name = S(m, "strMeal"),
                    Thumbnail = S(m, "strMealThumb"),
                    Area = S(m, "strArea"),
                    Instructions = S(m, "strInstructions"),
                    YoutubeUrl = S(m, "strYoutube"),
                    CategoryId = cat.Id,
                };

                for (var i = 1; i <= 20; i++)
                {
                    var name = S(m, $"strIngredient{i}").Trim();
                    if (name.Length == 0) continue;
                    meal.Ingredients.Add(new Ingredient
                    {
                        Name = name,
                        Measure = S(m, $"strMeasure{i}").Trim(),
                    });
                }

                db.Meals.Add(meal);
            }

            await db.SaveChangesAsync();
        }

        return Ok(new { categories = categories.Count, meals = await db.Meals.CountAsync() });
    }
}