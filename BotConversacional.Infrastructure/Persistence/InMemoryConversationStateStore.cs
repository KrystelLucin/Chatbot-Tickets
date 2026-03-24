using BotConversacional.Application.Interfaces;
using BotConversacional.Domain.Entities;

namespace BotConversacional.Infrastructure.Persistence;

public class InMemoryConversationStateStore : IConversationStateStore
{
    private static readonly Dictionary<string, ConversationState> Store = new();

    public ConversationState Get(string conversationId)
    {
        if (!Store.TryGetValue(conversationId, out var state))
        {
            state = new ConversationState();
            Store[conversationId] = state;
        }

        return state;
    }

    public void Save(string conversationId, ConversationState state)
    {
        Store[conversationId] = state;
    }

    public void Clear(string conversationId)
    {
        if (Store.ContainsKey(conversationId))
        {
            Store.Remove(conversationId);
        }
    }
}