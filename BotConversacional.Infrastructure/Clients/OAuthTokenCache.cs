using System.Threading;

namespace BotConversacional.Infrastructure.Clients;

public sealed class OAuthTokenCache
{
    public string? AccessToken { get; set; }
    public DateTime ExpiresAtUtc { get; set; } = DateTime.MinValue;

    public SemaphoreSlim Gate { get; } = new(1, 1);
}
