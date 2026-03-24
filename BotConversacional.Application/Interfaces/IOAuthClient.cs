namespace BotConversacional.Application.Interfaces;

public interface IOAuthClient
{
    Task<string> GetAccessTokenAsync(bool forceRefresh = false);
}