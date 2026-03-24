namespace BotConversacional.Domain.Enums;

public enum ConversationStep
{
    None = 0,
    AskName = 1,
    AskEmail = 2,
    AskDescription = 3,
    AwaitingConfirmation = 4
}