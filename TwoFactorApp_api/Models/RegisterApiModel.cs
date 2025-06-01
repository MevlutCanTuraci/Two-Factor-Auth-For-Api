using System.Security.Principal;

namespace TwoFactorApp_api.Models;

public class RegisterApiModel
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string Email { get; set; }
    public required bool EnableTwoFactor { get; set; } = false;
}