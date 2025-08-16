using System.Collections.Concurrent;
using System.Text.RegularExpressions;

// ↓ ↓ ↓ ESTA DECLARACIÓN DEBE IR AQUÍ, justo después de los using ↓ ↓ ↓
public record ShortenRequest(string longUrl);

// TODO el resto del código va después
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var urlMap = new ConcurrentDictionary<string, string>();

bool IsValidUrl(string url) =>
    Regex.IsMatch(url, @"^https:\/\/\S+$", RegexOptions.IgnoreCase);

app.MapPost("/api/shorten", (ShortenRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.longUrl) || !IsValidUrl(req.longUrl))
        return Results.BadRequest(new { error = "URL inválida" });

    var shortCode = Guid.NewGuid().ToString("n")[..8];
    urlMap[shortCode] = req.longUrl;

    var shortUrl = $"https://azurlshortener-brdjdsh6d3bebuer.canadacentral-01.azurewebsites.net/{shortCode}";
    return Results.Ok(new { shortUrl, originalUrl = req.longUrl });
});

app.MapGet("/{shortCode}", (string shortCode) =>
{
    if (urlMap.TryGetValue(shortCode, out var longUrl))
    {
        return Results.Redirect(longUrl);
    }
    return Results.NotFound("URL no encontrada");
});

app.Run();

