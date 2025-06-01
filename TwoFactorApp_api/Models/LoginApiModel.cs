namespace TwoFactorApp_api.Models;

public class LoginApiModel
{
    public required string Username { get; set; }
    public required string Password { get; set; }

    public string? TwoFactorCode { get; set; }
}