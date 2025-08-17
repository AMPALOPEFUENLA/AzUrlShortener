using System.Collections.Concurrent;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

// ✅ Habilitar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ✅ Activar la política CORS
app.UseCors("AllowAll");

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
        return Results.Redirect(longUrl);

    return Results.NotFound("URL no encontrada");
});

app.Run();
