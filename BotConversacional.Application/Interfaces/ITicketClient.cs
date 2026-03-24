using BotConversacional.Domain.Entities;

namespace BotConversacional.Application.Interfaces;

public interface ITicketClient
{
    Task<string> CreateTicketAsync(TicketDraft ticketDraft);
    Task<string> GetTicketStatusAsync(string ticketId);
}