using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Force explicit URL to avoid port collisions (dev) — Gateway listens on 7000
builder.WebHost.UseUrls("http://localhost:7000");

// Load Ocelot configuration from ocelot.json
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

// In dev allow HTTP; for production use HTTPS with proper certs
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "gateway" }));

await app.UseOcelot();

app.Run();
