using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using KobraKai.API.Models;            // ApplicationUser
using KobraKai.API.Dtos;                   // your DTO namespaces if needed (RegisterRequest, LoginRequest, MeResponse)
using KobraKai.API.Services;

namespace KobraKai.API.Controllers
{
    [ApiController]
    [Route("api/auth")] // explicit to avoid any controller-name coupling
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _users;
        private readonly RoleManager<IdentityRole> _roles;
        private readonly JwtTokenService _tokens;

        public AuthController(UserManager<ApplicationUser> users,
                              RoleManager<IdentityRole> roles,
                              JwtTokenService tokens)
        {
            _users = users;
            _roles = roles;
            _tokens = tokens;
        }

        // POST: /api/auth/register
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            if (req is null) return BadRequest(new { error = "Missing body" });

            var role = (req.Role ?? "").Trim().ToLowerInvariant();
            if (role != "beneficiary" && role != "member")
                return BadRequest(new { error = "Invalid role. Use 'beneficiary' or 'member'." });

            var user = new ApplicationUser
            {
                UserName = req.Email,
                Email = req.Email,
                Role = role,
                TokenBalance = role == "beneficiary" ? 10 : 0
            };

            var created = await _users.CreateAsync(user, req.Password);
            if (!created.Succeeded)
                return BadRequest(new { error = string.Join("; ", created.Errors.Select(e => e.Description)) });

            // Ensure Identity role exists & assign (keeps DB roles consistent with JWT claim)
            if (!await _roles.RoleExistsAsync(role))
            {
                var rc = await _roles.CreateAsync(new IdentityRole(role));
                if (!rc.Succeeded)
                    return BadRequest(new { error = "Failed creating role: " + string.Join("; ", rc.Errors.Select(e => e.Description)) });
            }

            var addToRole = await _users.AddToRoleAsync(user, role);
            if (!addToRole.Succeeded)
                return BadRequest(new { error = "User created, but failed to add to role: " + string.Join("; ", addToRole.Errors.Select(e => e.Description)) });

            return Created($"/api/auth/users/{user.Id}", new { ok = true, id = user.Id, role });
        }

        // POST: /api/auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            if (req is null) return BadRequest(new { error = "Missing body" });
            var user = await _users.FindByEmailAsync(req.Email);
            if (user is null) return Unauthorized();

            var ok = await _users.CheckPasswordAsync(user, req.Password);
            if (!ok) return Unauthorized();

            var token = _tokens.CreateToken(user); // already adds role/email/sub claims
            return Ok(new { token });
        }

        // GET: /api/auth/me
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<MeResponse>> Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _users.FindByIdAsync(userId);
            if (user is null) return NotFound();

            // Use the positional-record constructor
            return new MeResponse(user.Id, user.Email ?? "", user.Role, user.TokenBalance, user.UserName);
        }
    }
}
