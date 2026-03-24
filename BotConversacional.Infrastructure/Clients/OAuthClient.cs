using System.Net.Http.Json;
using BotConversacional.Application.Interfaces;

namespace BotConversacional.Infrastructure.Clients;

public class OAuthClient : IOAuthClient
{
    private readonly HttpClient _httpClient;
    private readonly OAuthTokenCache _tokenCache;

    public OAuthClient(HttpClient httpClient, OAuthTokenCache tokenCache)
    {
        _httpClient = httpClient;
        _tokenCache = tokenCache;
    }

    public async Task<string> GetAccessTokenAsync(bool forceRefresh = false)
    {
        var now = DateTime.UtcNow;

        if (!forceRefresh &&
            !string.IsNullOrWhiteSpace(_tokenCache.AccessToken) &&
            _tokenCache.ExpiresAtUtc > now)
        {
            return _tokenCache.AccessToken;
        }

        await _tokenCache.Gate.WaitAsync();

        try
        {
            now = DateTime.UtcNow;

            if (!forceRefresh &&
                !string.IsNullOrWhiteSpace(_tokenCache.AccessToken) &&
                _tokenCache.ExpiresAtUtc > now)
            {
                return _tokenCache.AccessToken;
            }

            var request = new TokenRequest
            {
                ClientId = "bot-client",
                ClientSecret = "super-secret",
                GrantType = "client_credentials"
            };

            var response = await _httpClient.PostAsJsonAsync("/oauth/token", request);
            response.EnsureSuccessStatusCode();

            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();

            if (tokenResponse is null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
            {
                throw new InvalidOperationException("No se pudo obtener un access token valido.");
            }

            _tokenCache.AccessToken = tokenResponse.AccessToken;

            var expiresIn = tokenResponse.ExpiresIn <= 0 ? 300 : tokenResponse.ExpiresIn;
            _tokenCache.ExpiresAtUtc = DateTime.UtcNow.AddSeconds(expiresIn - 30);

            return _tokenCache.AccessToken;
        }
        finally
        {
            _tokenCache.Gate.Release();
        }
    }

    private sealed class TokenRequest
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string GrantType { get; set; } = string.Empty;
    }

    private sealed class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
        public string TokenType { get; set; } = string.Empty;
    }
}
