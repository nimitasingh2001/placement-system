using System.ComponentModel.DataAnnotations;

namespace PlacementApi.Dtos;

public class RegisterRequest
{
    [Required, StringLength(20)]
    public string RollNo { get; set; } = string.Empty;

    [Required, StringLength(100), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;
}

public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public record AuthResponse(string Token, DateTime ExpiresAtUtc, string Role, int? StudentId);

public record MeResponse(int UserId, string Email, string Role, int? StudentId);