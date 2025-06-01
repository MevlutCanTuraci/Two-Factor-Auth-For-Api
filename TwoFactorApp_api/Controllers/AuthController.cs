using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TwoFactorApp_api.Helpers;
using TwoFactorApp_api.Models;
namespace TwoFactorApp_api.Controllers;


[ApiController, Route("api/[controller]")]
public class AuthController : Controller
{
    private static List<UsersModel> _users = new List<UsersModel>
    {
        new UsersModel
        {
            Username = "user",
            Password = "1",
            Email = "example@domain.com",
            EnableTwoFactor = true,
            SecretKey = "HA3LDEVR5AGEXAZZRBQLJQQW5GDMXYGQ"
        }
    };

    private readonly TwoFactorService _twoFactorService;

    public AuthController(TwoFactorService twoFactorService)
    {
        _twoFactorService = twoFactorService;
    }
    

    [HttpPost("Login")]
    public IActionResult Login(
        [FromBody] LoginApiModel model
    )
    {
        var alreadyExists = _users.Any(x => x.Username == model.Username && x.Password == model.Password);
        if (!alreadyExists)
        {
            return BadRequest(new
            {
                Message = "Invalid username or password."
            });
        }
        
        var user = _users.FirstOrDefault(x => x.Username == model.Username);

        if (user!.EnableTwoFactor)
        {
            if (string.IsNullOrEmpty(model.TwoFactorCode?.Trim()))
            {
                return StatusCode(419, new 
                {
                    Message = "Two-factor authentication is enabled for this user. Please provide a valid two-factor code."
                });
            }
            
            else if (user.EnableTwoFactor && !string.IsNullOrEmpty(model.TwoFactorCode?.Trim()))
            {
                var verified = _twoFactorService.VerifyCode(model.TwoFactorCode, user.SecretKey!);
                if (!verified)
                {
                    return BadRequest(new
                    {
                        Message = "Invalid two-factor code."
                    });
                }
            }
        }
        
        return Ok(new
        {
            Message = "Login successful",
        });
    }
    
    
    [HttpPost("Register")]
    public IActionResult Register(
        [FromBody] RegisterApiModel model
    )
    {
        var alreadyExists = _users.Any(x => x.Username == model.Username);
        if (alreadyExists)
        {
            return BadRequest(new
            {
                Message = "Username already exists."
            });
        }
        
        var secretKey = _twoFactorService.GenerateSecret();
        
        _users.Add(new UsersModel
        {
            Username = model.Username,
            Password = model.Password,
            Email = model.Email,
            EnableTwoFactor = model.EnableTwoFactor,
            SecretKey = model.EnableTwoFactor ? secretKey : null
        });
        
        return Ok(new
        {
            Message = "Registration successful",
        });
    }
    
    
    [HttpGet("/api/User")]
    public IActionResult Get(
        [FromQuery, Required] string username,
        [FromQuery] bool showQRCode = false
    )
    {
        var user = _users.FirstOrDefault(x => x.Username == username);
        if (user == null)
        {
            return NotFound(new
            {
                Message = "User not found."
            });
        }

        if (showQRCode && user.EnableTwoFactor)
        {
            var qrCodeStream = _twoFactorService.GenerateQrCodeUri(user.Email, user.SecretKey!);
            return File(qrCodeStream, "image/png");
        }
        else
        {
            return Ok(user);   
        }
    }

}