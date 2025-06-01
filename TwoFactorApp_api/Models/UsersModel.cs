namespace TwoFactorApp_api.Models;

public class UsersModel
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string Email { get; set; }
    public string? SecretKey { get; set; } = null;
    public bool EnableTwoFactor { get; set; } = false;
}