using KobraKai.Api.Services;
using KobraKai.API.Data;
using KobraKai.API.Dtos;
using KobraKai.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace KobraKai.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly SignInManager<ApplicationUser> _signin;
    private readonly JwtTokenService _jwt;
    private readonly AppDbContext _db;

    public AuthController(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signin, JwtTokenService jwt, AppDbContext db)
    {
        _users = users; _signin = signin; _jwt = jwt; _db = db;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        if (req.Role != "beneficiary" && req.Role != "member")
            return BadRequest(new { error = "Invalid role" });

        var user = new ApplicationUser
        {
            UserName = req.Email,
            Email = req.Email,
            Role = req.Role,
            TokenBalance = req.Role == "beneficiary" ? 10 : 0
        };
        var result = await _users.CreateAsync(user, req.Password);
        if (!result.Succeeded) return BadRequest(new { error = string.Join("; ", result.Errors.Select(e => e.Description)) });

        // Optionally add IdentityRole & role assignment here if you want formal roles
        return Ok(new { ok = true, id = user.Id });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await _users.FindByEmailAsync(req.Email);
        if (user == null) return Unauthorized(new { error = "Invalid credentials" });

        var pwOk = await _users.CheckPasswordAsync(user, req.Password);
        if (!pwOk) return Unauthorized(new { error = "Invalid credentials" });

        var token = _jwt.Create(user);
        return Ok(new { token });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<MeResponse>> Me()
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Unauthorized();
        return new MeResponse(user.Id, user.Email!, user.Role, user.TokenBalance, user.UserName);
    }
}
