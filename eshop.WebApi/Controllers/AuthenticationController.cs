using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace eshop.WebApi.Controllers;

[ApiController]
[Route("/")]
public class AuthenticationController : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult> LoginAsync([FromForm]string email, [FromForm]string password)
    {
        var claims = new List<Claim> { new(ClaimTypes.Name, email) };
        var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
        
        await  HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
    
        return Redirect("/");
    }

    [HttpPost("logout")]
    public async Task<ActionResult> LogoutAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
    }
}