using BotConversacional.Application.DTOs;
using BotConversacional.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BotConversacional.Api.Controllers;

[ApiController]
[Route("messages")]
public class MessagesController : ControllerBase
{
    private readonly ConversationService _conversationService;

    public MessagesController(ConversationService conversationService)
    {
        _conversationService = conversationService;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] MessageRequest request)
    {
        var response = await _conversationService.HandleMessageAsync(request);
        return Ok(response);
    }
}