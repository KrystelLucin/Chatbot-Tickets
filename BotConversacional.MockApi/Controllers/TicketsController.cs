using BotConversacional.MockApi.Models;
using BotConversacional.MockApi.Storage;
using Microsoft.AspNetCore.Mvc;

namespace BotConversacional.MockApi.Controllers;

[ApiController]
[Route("tickets")]
public class TicketsController : ControllerBase
{
    private static int _forcedUnauthorizedOnce;

    [HttpPost]
    public IActionResult Create([FromBody] CreateTicketRequest request)
    {
        if (!HasValidBearerToken())
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Description))
        {
            return BadRequest("Todos los campos son obligatorios.");
        }

        var ticket = TicketStore.Create(request);
        return Ok(ticket);
    }

    [HttpGet("{id}")]
    public IActionResult GetById(string id)
    {
        if (!HasValidBearerToken())
        {
            return Unauthorized();
        }

        var ticket = TicketStore.GetById(id);

        if (ticket is null)
        {
            return NotFound();
        }

        return Ok(ticket);
    }

    private bool HasValidBearerToken()
    {
        var authHeader = Request.Headers.Authorization.ToString();

        if (string.IsNullOrWhiteSpace(authHeader))
        {
            return false;
        }

        var forceUnauthorizedOnce = string.Equals(
            Environment.GetEnvironmentVariable("MOCKAPI_FORCE_401_ONCE"),
            "true",
            StringComparison.OrdinalIgnoreCase);

        if (forceUnauthorizedOnce &&
            System.Threading.Interlocked.CompareExchange(ref _forcedUnauthorizedOnce, 1, 0) == 0)
        {
            return false;
        }

        return authHeader == "Bearer mock-valid-token";
    }
}
