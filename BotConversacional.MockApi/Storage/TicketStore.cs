using BotConversacional.MockApi.Models;

namespace BotConversacional.MockApi.Storage;

public static class TicketStore
{
    private static readonly Dictionary<string, TicketResponse> Tickets = new();

    public static TicketResponse Create(CreateTicketRequest request)
    {
        var id = $"TCK-{Random.Shared.Next(100, 999)}";

        var ticket = new TicketResponse
        {
            Id = id,
            Status = "Created"
        };

        Tickets[id] = ticket;
        return ticket;
    }

    public static TicketResponse? GetById(string id)
    {
        Tickets.TryGetValue(id, out var ticket);
        return ticket;
    }
}