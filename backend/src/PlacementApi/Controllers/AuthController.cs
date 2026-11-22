using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using PlacementApi.Auth;
using PlacementApi.Dtos;
using PlacementApi.Services;

namespace PlacementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    [EnableRateLimiting("auth")]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken ct)
    {
        await _auth.RegisterStudentAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, new { message = "Account created. You can now log in." });
    }

    [EnableRateLimiting("auth")]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
        => await _auth.LoginAsync(request, ct);

    [Authorize]
    [HttpGet("me")]
    public ActionResult<MeResponse> Me()
        => new MeResponse(
            User.GetUserId(),
            User.FindFirst("email")?.Value ?? string.Empty,
            User.FindFirst("role")?.Value ?? string.Empty,
            User.GetStudentId());
}