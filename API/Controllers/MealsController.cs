// MealsController.cs
using KobraKai.API.Data;
using KobraKai.API.Dtos;
using KobraKai.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KobraKai.API.Controllers // ⬅︎ match Auth/Orders
{
    [ApiController]
    [Route("api/meals")] // plural
    public class MealsController : ControllerBase // ⬅︎ rename class to plural (optional but tidy)
    {
        private readonly AppDbContext _db;
        public MealsController(AppDbContext db) => _db = db;

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<MealDto>>> GetAll()
        {
            var entities = await _db.Meals.AsNoTracking().ToListAsync();
            var items = entities.Select(ToDto).ToList();
            return Ok(items);
        }

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<ActionResult<MealDto>> GetById(Guid id)
            => (await _db.Meals.FindAsync(id)) is { } m ? Ok(ToDto(m)) : NotFound();

        [HttpPost]
        [Authorize(Roles = "member")]
        public async Task<ActionResult<MealDto>> Create([FromBody] MealCreateDto input)
        {
            var entity = new Meal
            {
                Id = Guid.NewGuid(),
                Title = input.Title,
                Description = input.Description,
                DietaryTagsCsv = string.Join(',', input.DietaryTags ?? Array.Empty<string>()),
                PortionsAvailable = input.PortionsAvailable,
            };
            _db.Meals.Add(entity);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, ToDto(entity));
        }

        private static MealDto ToDto(Meal m) => new()
        {
            Id = m.Id,
            Title = m.Title,
            Description = m.Description,
            DietaryTags = string.IsNullOrWhiteSpace(m.DietaryTagsCsv)
                ? Array.Empty<string>()
                : m.DietaryTagsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            PortionsAvailable = m.PortionsAvailable,
            ProviderId = m.ProviderId
        };
    }
}