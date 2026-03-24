using BotConversacional.Domain.Enums;

namespace BotConversacional.Domain.Entities;

public class ConversationState
{
    public ActiveFlow ActiveFlow { get; set; } = ActiveFlow.None;
    public ConversationStep CurrentStep { get; set; } = ConversationStep.None;
    public TicketDraft TicketDraft { get; set; } = new();
}