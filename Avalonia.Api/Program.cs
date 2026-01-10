using Avalonia.Api.Data;
using Avalonia.Api.Hubs;
using Avalonia.Api.Services;
using Avalonia.Shared.Enums;
using Avalonia.Shared.Interfaces;
using Avalonia.Shared.ModelDtos;
using Avalonia.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSignalR();
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseSqlite("Data Source=todo.db"));

// Authentication/Authorization (dev symmetric key; align with AuthService)
const string issuer = "Avalonia.AuthService";
const string audience = "Avalonia.Clients";
var signingKey = new SymmetricSecurityKey("dev-super-secret-key-please-change-1234567890"u8.ToArray());

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // dev only
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = signingKey,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30)
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            
            if (!string.IsNullOrEmpty(accessToken)
                && path.StartsWithSegments("/hubs/todo"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

// Register remote logger
builder.Services.AddHttpClient<IRemoteLogger, RemoteLogger>(c =>
{
    c.BaseAddress = new Uri("http://localhost:5115");
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Ensure database exists for development. Use EnsureCreated to avoid pending-migrations exception in dev.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ToDoDbContext>();
    db.Database.EnsureCreated();
}

// Swagger
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Avalonia Web API v1");
        options.RoutePrefix = string.Empty;
    });
}

// For local development behind a gateway using HTTP, skip HTTPS redirection.
// Enable HTTPS in production with proper certificates.
// app.UseHttpsRedirection();
app.UseWebSockets();
app.UseAuthentication();
app.UseAuthorization();

# region Endpoints

// Get all
app.MapGet("/api/todo", async (ToDoDbContext db, IRemoteLogger logger) =>
{
    await logger.LogAsync("[GET] Fetched all tasks.");
    
    return await db.ToDos
        .OrderByDescending(t => t.CreatedAt)
        .Select(t => new ToDoDto
        {
            Id = t.Id,
            CreatedAt = t.CreatedAt,
            Title = t.Title,
            Description = t.Description,
            IsCompleted = t.IsCompleted
        })
        .ToListAsync();
}).RequireAuthorization();

// Get by ID
app.MapGet("/api/todo/{id:int}", async (int id, ToDoDbContext db, IRemoteLogger logger) =>
{
    var t = await db.ToDos.FindAsync(id);
    await logger.LogAsync($"[GET] Fetched task Id: {id}");
    
    return t is null ? Results.NotFound() : Results.Ok(new ToDoDto
    {
        Id = t.Id,
        CreatedAt = t.CreatedAt,
        Title = t.Title,
        Description = t.Description,
        IsCompleted = t.IsCompleted
    });
}).RequireAuthorization();

// Create
app.MapPost("/api/todo", async (ToDoDto dto, ToDoDbContext db, IRemoteLogger logger, IHubContext<ToDoHub> hubContext) =>
{
    var todo = new ToDo
    {
        CreatedAt = dto.CreatedAt == default ? DateTime.UtcNow : dto.CreatedAt,
        Title = dto.Title,
        Description = dto.Description,
        IsCompleted = dto.IsCompleted
    };

    db.ToDos.Add(todo);
    await db.SaveChangesAsync();

    await logger.LogAsync($"[POST] Created task Id: {todo.Id} - {todo.Title}", LogType.Create);
    await hubContext.Clients.All.SendAsync("ToDoUpdated", "Create", todo.Id);

    return Results.Created($"/api/todo/{todo.Id}", new ToDoDto
    {
        Id = todo.Id,
        CreatedAt = todo.CreatedAt,
        Title = todo.Title,
        Description = todo.Description,
        IsCompleted = todo.IsCompleted
    });
}).RequireAuthorization();

// Update
app.MapPut("/api/todo/{id:int}", async (int id, ToDoDto dto, ToDoDbContext db, IRemoteLogger logger, IHubContext<ToDoHub> hubContext) =>
{
    var t = await db.ToDos.FindAsync(id);
    if (t is null) return Results.NotFound();

    t.Title = dto.Title;
    t.Description = dto.Description;
    t.IsCompleted = dto.IsCompleted;

    await db.SaveChangesAsync();

    await logger.LogAsync($"[PUT] Updated task Id: {id}", LogType.Update);
    await hubContext.Clients.All.SendAsync("ToDoUpdated", "Update", id);

    return Results.Ok(dto);
}).RequireAuthorization();

// Delete
app.MapDelete("/api/todo/{id:int}", async (int id, ToDoDbContext db, IRemoteLogger logger, IHubContext<ToDoHub> hubContext) =>
{
    var t = await db.ToDos.FindAsync(id);
    if (t is null) return Results.NotFound();

    db.ToDos.Remove(t);
    await db.SaveChangesAsync();

    await logger.LogAsync($"[DELETE] Deleted task Id: {id}", LogType.Delete);
    await hubContext.Clients.All.SendAsync("ToDoUpdated", "Delete", id);

    return Results.NoContent();
}).RequireAuthorization();

# endregion

app.MapHub<ToDoHub>("/hubs/todo");

app.Run();