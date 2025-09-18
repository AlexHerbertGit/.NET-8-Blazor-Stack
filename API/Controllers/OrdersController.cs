using KobraKai.API.Data;
using KobraKai.API.Dtos;
using KobraKai.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace KobraKai.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public OrdersController(AppDbContext db, UserManager<ApplicationUser> users)
    {
        _db = db; _users = users;
    }

    // Beneficiary places order (spends 1 token & decrements meal portions)
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<OrderDto>> Place([FromBody] PlaceOrderDto dto)
    {
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "";
        if (role != "beneficiary") return Forbid();

        var beneficiaryId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await _users.FindByIdAsync(beneficiaryId);
        if (user == null) return Unauthorized();

        var meal = await _db.Meals.FirstOrDefaultAsync(m => m.Id == dto.MealId);
        if (meal == null) return NotFound(new { error = "Meal not found" });

        if (user.TokenBalance <= 0) return BadRequest(new { error = "Insufficient tokens" });
        if (meal.PortionsAvailable <= 0) return BadRequest(new { error = "No portions available" });

        user.TokenBalance -= 1;
        meal.PortionsAvailable -= 1;

        var order = new Order
        {
            Id = Guid.NewGuid(),
            BeneficiaryId = beneficiaryId,
            MemberId = meal.ProviderId,
            MealId = meal.Id,
            Status = "Pending",
            OrderedAt = DateTime.UtcNow
        };

        _db.Orders.Add(order);
        await _users.UpdateAsync(user);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMine), new { role = "beneficiary" },
            new OrderDto(order.Id, order.BeneficiaryId, order.MemberId, order.MealId, order.Status, order.OrderedAt));
    }

    // Member accepts order
    [Authorize]
    [HttpPost("{id:guid}/accept")]
    public async Task<ActionResult<OrderDto>> Accept(Guid id)
    {
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "";
        if (role != "member") return Forbid();

        var memberId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id && o.MemberId == memberId);
        if (order == null) return NotFound(new { error = "Order not found" });

        order.Status = "Accepted";
        await _db.SaveChangesAsync();

        return new OrderDto(order.Id, order.BeneficiaryId, order.MemberId, order.MealId, order.Status, order.OrderedAt);
    }

    // List my orders (by role)
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetMine([FromQuery] string role)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        IQueryable<Order> q = _db.Orders.AsNoTracking();

        if (role == "beneficiary") q = q.Where(o => o.BeneficiaryId == userId);
        else if (role == "member") q = q.Where(o => o.MemberId == userId);
        else return BadRequest(new { error = "role must be 'beneficiary' or 'member'" });

        var items = await q.OrderByDescending(o => o.OrderedAt).ToListAsync();
        return items.Select(o => new OrderDto(o.Id, o.BeneficiaryId, o.MemberId, o.MealId, o.Status, o.OrderedAt)).ToList();
    }
}