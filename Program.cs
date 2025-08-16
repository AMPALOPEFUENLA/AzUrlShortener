using System.Collections.Concurrent;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Diccionario en memoria para guardar el mapeo de códigos cortos a URLs largas
var urlMap = new ConcurrentDictionary<string, string>();

// Regex simple para validar URLs (opcional)
bool IsValidUrl(string url) => 
    Regex.IsMatch(url, @"^https:\/\/\S+$", RegexOptions.IgnoreCase);

// Endpoint API para crear un enlace corto
app.MapPost("/api/shorten", (ShortenRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.longUrl) || !IsValidUrl(req.longUrl))
        return Results.BadRequest(new { error = "URL inválida" });

    // Genera un código corto único
    var shortCode = Guid.NewGuid().ToString("n")[..8];
    urlMap[shortCode] = req.longUrl;

    // ¡Cambia la URL base por la tuya!
    var shortUrl = $"https://azurlshortener-brdjdsh6d3bebuer.canadacentral-01.azurewebsites.net/{shortCode}";
    return Results.Ok(new { shortUrl, originalUrl = req.longUrl });
});

// Endpoint para redirigir con el código corto
app.MapGet("/{shortCode}", (string shortCode) =>
{
    if (urlMap.TryGetValue(shortCode, out var longUrl))
    {
        return Results.Redirect(longUrl);
    }
    return Results.NotFound("URL no encontrada");
});

// Modelo para la petición de acortado
public record ShortenRequest(string longUrl);

app.Run();
