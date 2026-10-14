using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeApi.Data;
using RecipeApi.Dtos;
using RecipeApi.Models;

namespace RecipeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MealsController(AppDbContext db) : ControllerBase
{
    // GET /api/meals?search=chicken&category=Seafood
    [HttpGet]
    public async Task<ActionResult<List<MealSummaryDto>>> Get(
        [FromQuery] string? search, [FromQuery] string? category)
    {
        var query = db.Meals.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(m => m.Name.Contains(search));   // SQL: LIKE '%search%'

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(m => m.Category!.Name == category);

        return await query
            .OrderBy(m => m.Name)
            .Select(m => new MealSummaryDto(m.Id, m.Name, m.Thumbnail))
            .ToListAsync();
    }

    // GET /api/meals/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<MealDto>> GetById(int id)
    {
        var meal = await db.Meals
            .Include(m => m.Category)
            .Include(m => m.Ingredients)
            .FirstOrDefaultAsync(m => m.Id == id);

        return meal is null ? NotFound() : ToDto(meal);
    }

    // GET /api/meals/random
    [HttpGet("random")]
    public async Task<ActionResult<MealDto>> GetRandom()
    {
        var count = await db.Meals.CountAsync();
        if (count == 0) return NotFound();

        var meal = await db.Meals
            .Include(m => m.Category)
            .Include(m => m.Ingredients)
            .OrderBy(m => m.Id)
            .Skip(Random.Shared.Next(count))
            .FirstAsync();

        return ToDto(meal);
    }

    private static MealDto ToDto(Meal m) => new(
        m.Id, m.Name, m.Thumbnail, m.Category?.Name ?? "", m.Area,
        m.Instructions, m.YoutubeUrl,
        m.Ingredients.Select(i => new IngredientDto(i.Name, i.Measure)).ToList());
}