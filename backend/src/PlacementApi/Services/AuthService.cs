using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PlacementApi.Auth;
using PlacementApi.Data;
using PlacementApi.Data.Entities;
using PlacementApi.Dtos;
using PlacementApi.Infrastructure;

namespace PlacementApi.Services;

public interface IAuthService
{
    Task RegisterStudentAsync(RegisterRequest request, CancellationToken ct);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct);
}

public class AuthService : IAuthService
{
    private readonly PlacementDbContext _db;
    private readonly ITokenService _tokens;
    private readonly IPasswordHasher<AppUser> _hasher;

    public AuthService(PlacementDbContext db, ITokenService tokens, IPasswordHasher<AppUser> hasher)
    {
        _db = db;
        _tokens = tokens;
        _hasher = hasher;
    }

    public async Task RegisterStudentAsync(RegisterRequest request, CancellationToken ct)
    {
        var rollNo = request.RollNo.Trim();
        var email = request.Email.Trim();

        // the student record already exists (added by the college); the roll number AND email must both match
        var student = await _db.Students
            .FirstOrDefaultAsync(s => s.RollNo == rollNo && s.Email == email && s.IsActive, ct);
        if (student is null)
            throw new ApiException(400, "No active student matches that roll number and email.");

        var alreadyRegistered = await _db.AppUsers
            .AnyAsync(u => u.StudentId == student.StudentId || u.Email == student.Email, ct);
        if (alreadyRegistered)
            throw new ApiException(409, "An account already exists for this student.");

        var user = new AppUser
        {
            Email = student.Email,
            Role = "Student",
            StudentId = student.StudentId,
            PasswordHash = string.Empty
        };
        user.PasswordHash = _hasher.HashPassword(user, request.Password);

        _db.AppUsers.Add(user);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim();
        var user = await _db.AppUsers.FirstOrDefaultAsync(u => u.Email == email, ct);

        // same message for "unknown email" and "wrong password", so attackers cannot probe which emails exist
        if (user is null)
            throw new ApiException(401, "Invalid email or password.");

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            throw new ApiException(401, "Invalid email or password.");

        var (token, expires) = _tokens.CreateToken(user);
        return new AuthResponse(token, expires, user.Role, user.StudentId);
    }
}