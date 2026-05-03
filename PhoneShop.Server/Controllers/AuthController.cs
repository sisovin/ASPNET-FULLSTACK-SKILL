using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhoneShop.Server.Data;
using PhoneShop.Server.Services;
using PhoneShop.Shared.DTOs;
using PhoneShop.Shared.Models;

namespace PhoneShop.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly PhoneShopDbContext _db;
    private readonly IPasswordHasher<ApplicationUser> _hasher;
    private readonly ITokenService _tokenService;

    public AuthController(
        PhoneShopDbContext db,
        IPasswordHasher<ApplicationUser> hasher,
        ITokenService tokenService)
    {
        _db           = db;
        _hasher       = hasher;
        _tokenService = tokenService;
    }

    /// <summary>Register a new user account and return an auth token.</summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            return BadRequest("Email is already registered.");

        var user = new ApplicationUser
        {
            Email     = request.Email,
            FullName  = request.FullName,
            Role      = "User",
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = _hasher.HashPassword(user, request.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok(_tokenService.GenerateToken(user));
    }

    /// <summary>Authenticate with email + password and return an auth token.</summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user is null)
            return Unauthorized("Invalid credentials.");

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            return Unauthorized("Invalid credentials.");

        return Ok(_tokenService.GenerateToken(user));
    }
}
