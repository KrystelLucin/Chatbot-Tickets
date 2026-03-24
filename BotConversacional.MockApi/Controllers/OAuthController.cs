using BotConversacional.MockApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace BotConversacional.MockApi.Controllers;

[ApiController]
[Route("oauth")]
public class OAuthController : ControllerBase
{
    [HttpPost("token")]
    public IActionResult GetToken([FromBody] TokenRequest request)
    {
        if (request.ClientId != "bot-client" ||
            request.ClientSecret != "super-secret" ||
            request.GrantType != "client_credentials")
        {
            return Unauthorized();
        }

        var response = new TokenResponse
        {
            AccessToken = "mock-valid-token",
            TokenType = "Bearer",
            ExpiresIn = 300
        };

        return Ok(response);
    }
}