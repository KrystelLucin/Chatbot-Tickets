using BotConversacional.Domain.Entities;

namespace BotConversacional.Application.Interfaces;

public interface IConversationStateStore
{
    ConversationState Get(string conversationId);
    void Save(string conversationId, ConversationState state);
    void Clear(string conversationId);
}