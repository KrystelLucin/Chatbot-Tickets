using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BotConversacional.Application.Interfaces;
using BotConversacional.Domain.Entities;

namespace BotConversacional.Infrastructure.Clients;

public class TicketClient : ITicketClient
{
    private readonly HttpClient _httpClient;
    private readonly IOAuthClient _oAuthClient;

    public TicketClient(HttpClient httpClient, IOAuthClient oAuthClient)
    {
        _httpClient = httpClient;
        _oAuthClient = oAuthClient;
    }

    public async Task<string> CreateTicketAsync(TicketDraft ticketDraft)
    {
        var request = new CreateTicketRequest
        {
            Name = ticketDraft.Name ?? string.Empty,
            Email = ticketDraft.Email ?? string.Empty,
            Description = ticketDraft.Description ?? string.Empty
        };

        var response = await SendAuthorizedPostAsync("/tickets", request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Error al crear ticket ({(int)response.StatusCode}): {error}");
        }

        var ticketResponse = await response.Content.ReadFromJsonAsync<CreateTicketResponse>();

        if (ticketResponse is null || string.IsNullOrWhiteSpace(ticketResponse.Id))
        {
            throw new InvalidOperationException("No se pudo obtener el ID del ticket creado.");
        }

        return ticketResponse.Id;
    }

    public async Task<string> GetTicketStatusAsync(string ticketId)
    {
        var response = await SendAuthorizedGetAsync($"/tickets/{ticketId}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return "NotFound";
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Error al consultar ticket ({(int)response.StatusCode}): {error}");
        }

        var ticketResponse = await response.Content.ReadFromJsonAsync<TicketStatusResponse>();

        if (ticketResponse is null || string.IsNullOrWhiteSpace(ticketResponse.Status))
        {
            throw new InvalidOperationException("No se pudo obtener el estado del ticket.");
        }

        return ticketResponse.Status;
    }

    private async Task<HttpResponseMessage> SendAuthorizedPostAsync<TBody>(string url, TBody body)
    {
        var token = await _oAuthClient.GetAccessTokenAsync();
        var response = await PostAsync(url, body, token);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            token = await _oAuthClient.GetAccessTokenAsync(forceRefresh: true);
            response = await PostAsync(url, body, token);
        }
        return response;
    }

    private async Task<HttpResponseMessage> SendAuthorizedGetAsync(string url)
    {
        var token = await _oAuthClient.GetAccessTokenAsync();
        var response = await GetAsync(url, token);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            token = await _oAuthClient.GetAccessTokenAsync(forceRefresh: true);
            response = await GetAsync(url, token);
        }
        return response;
    }

    private async Task<HttpResponseMessage> PostAsync<TBody>(string url, TBody body, string token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(body);

        return await _httpClient.SendAsync(request);
    }

    private async Task<HttpResponseMessage> GetAsync(string url, string token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await _httpClient.SendAsync(request);
    }

    private sealed class CreateTicketRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    private sealed class CreateTicketResponse
    {
        public string Id { get; set; } = string.Empty;
    }

    private sealed class TicketStatusResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
