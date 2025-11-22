using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog console logging
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();

builder.WebHost.UseUrls("http://localhost:5201");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Simple in-memory user store for dev
var users = new Dictionary<string, string>
{
    ["demo"] = "demo"
};

// Dev symmetric key (replace with RSA + proper Identity/OpenIddict in prod)
const string issuer = "Avalonia.AuthService";
const string audience = "Avalonia.Clients";
var signingKey = new SymmetricSecurityKey("dev-super-secret-key-please-change-1234567890"u8.ToArray());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapPost("/auth/token", ([FromBody] LoginRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
        return Results.BadRequest(new { error = "invalid_request" });

    if (!users.TryGetValue(req.Username, out var pwd) || pwd != req.Password)
        return Results.Unauthorized();

    var now = DateTimeOffset.UtcNow;
    var handler = new JwtSecurityTokenHandler();
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Issuer = issuer,
        Audience = audience,
        Expires = now.AddMinutes(30).UtcDateTime,
        NotBefore = now.UtcDateTime,
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, req.Username),
            new Claim("scope", "todo.read todo.write")
        }),
        SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
    };

    var token = handler.CreateToken(tokenDescriptor);
    var jwt = handler.WriteToken(token);
    return Results.Ok(new TokenResponse
    {
        AccessToken = jwt,
        TokenType = "Bearer",
        ExpiresIn = 1800
    });
});

app.MapGet("/auth/health", () => Results.Ok(new { status = "ok", service = "auth" }));

app.Run();

record LoginRequest(string Username, string Password);
record TokenResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string TokenType { get; init; } = "Bearer";
    public int ExpiresIn { get; init; }
}
