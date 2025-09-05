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
    public MealsController(AppDbContext db) { _db = db; }

    // Public list
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MealDto>>> List()
    {
        var items = await _db.Meals.AsNoTracking().ToListAsync();
        return items.Select(m => new MealDto(
            m.Id, m.Title, m.Description,
            (m.DietaryTagsCsv ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries),
            m.PortionsAvailable,
            m.ProviderId
        )).ToList();
    }

    // Member create
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<MealDto>> Create([FromBody] MealCreateDto dto)
    {
        // Quick role check from claim; you can also use [Authorize(Roles="member")] if using Identity roles
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "";
        if (role != "member") return Forbid();

        var providerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var meal = new Meal
        {
            Title = dto.Title,
            Description = dto.Description,
            DietaryTagsCsv = dto.DietaryTags is { Length: > 0 } ? string.Join(',', dto.DietaryTags) : "",
            PortionsAvailable = dto.PortionsAvailable,
            ProviderId = providerId
        };
        _db.Meals.Add(meal);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(List), new { id = meal.Id },
            new MealDto(meal.Id, meal.Title, meal.Description,
            (meal.DietaryTagsCsv ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries),
            meal.PortionsAvailable, meal.ProviderId));
    }
}