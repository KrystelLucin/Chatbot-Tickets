using BotConversacional.Application.Interfaces;
using BotConversacional.Application.Services;
using BotConversacional.Infrastructure.Clients;
using BotConversacional.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<ConversationService>();

builder.Services.AddSingleton<IConversationStateStore, InMemoryConversationStateStore>();

builder.Services.AddSingleton<OAuthTokenCache>();

var mockApiBaseUrlRaw = builder.Configuration["MockApi:BaseUrl"] ?? "http://localhost:5221";
if (!Uri.TryCreate(mockApiBaseUrlRaw, UriKind.Absolute, out var mockApiBaseUrl))
{
    throw new InvalidOperationException($"MockApi:BaseUrl no es una URL valida: '{mockApiBaseUrlRaw}'");
}

builder.Services.AddHttpClient<IOAuthClient, OAuthClient>(client =>
{
    client.BaseAddress = mockApiBaseUrl;
});

builder.Services.AddHttpClient<ITicketClient, TicketClient>(client =>
{
    client.BaseAddress = mockApiBaseUrl;
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();
