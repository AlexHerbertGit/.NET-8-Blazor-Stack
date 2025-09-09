using KobraKai.API.Data;
using KobraKai.API.Dtos;
using KobraKai.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KobraKai.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MealsController : ControllerBase
{
    private readonly AppDbContext _db;

    public MealsController(AppDbContext db) => _db = db;

    // GET: api/meals
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MealDto>>> GetAll()
    {
        var meals = await _db.Meals.AsNoTracking().ToListAsync();
        return Ok(meals.Select(ToDto));
    }

    // GET: api/meals/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MealDto>> GetById(Guid id)
    {
        var meal = await _db.Meals.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
        if (meal is null) return NotFound();
        return Ok(ToDto(meal));
    }

    // POST: api/meals  (members only)
    [HttpPost]
    [Authorize(Roles = "member")]
    public async Task<ActionResult<MealDto>> Create(MealCreateDto input)
    {
        var entity = new Meal
        {
            Id = Guid.NewGuid(),
            Title = input.Title,
            Description = input.Description,
            DietaryTagsCsv = JoinCsv(input.DietaryTags),
            PortionsAvailable = input.PortionsAvailable,
            // Optionally set ProviderId from the logged-in user:
            // ProviderId = User.FindFirstValue(ClaimTypes.NameIdentifier)
        };

        _db.Meals.Add(entity);
        await _db.SaveChangesAsync();

        var dto = ToDto(entity);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, dto);
    }

    // PUT: api/meals/{id}  (members only)
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "member")]
    public async Task<IActionResult> Update(Guid id, MealUpdateDto input)
    {
        var meal = await _db.Meals.FindAsync(id);
        if (meal is null) return NotFound();

        meal.Title = input.Title;
        meal.Description = input.Description;
        meal.DietaryTagsCsv = JoinCsv(input.DietaryTags);
        meal.PortionsAvailable = input.PortionsAvailable;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/meals/{id}  (members only)
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "member")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var meal = await _db.Meals.FindAsync(id);
        if (meal is null) return NotFound();

        _db.Meals.Remove(meal);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ----------------- helpers -----------------

    private static MealDto ToDto(Meal m) => new()
    {
        Id = m.Id,
        Title = m.Title,
        Description = m.Description,
        DietaryTags = SplitCsv(m.DietaryTagsCsv),
        PortionsAvailable = m.PortionsAvailable,
        ProviderId = m.ProviderId
    };

    private static string[] SplitCsv(string? csv) =>
        string.IsNullOrWhiteSpace(csv)
            ? Array.Empty<string>()
            : csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static string JoinCsv(IEnumerable<string>? tags) =>
        tags is null ? "" : string.Join(",", tags);
}
