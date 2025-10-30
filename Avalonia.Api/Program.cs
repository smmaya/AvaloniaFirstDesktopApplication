using Avalonia.Api.Data;
using Avalonia.Shared.ModelDtos;
using Avalonia.Shared.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// --- Services ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseSqlite("Data Source=todo.db"));

var app = builder.Build();

// --- Middleware ---
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Avalonia Web API v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

// --- Endpoints ---

// Get all
app.MapGet("/api/todo", async (ToDoDbContext db) =>
    await db.ToDos
        .OrderByDescending(t => t.CreatedAt)
        .Select(t => new ToDoDto
        {
            Id = t.Id,
            CreatedAt = t.CreatedAt,
            Title = t.Title,
            Description = t.Description,
            IsCompleted = t.IsCompleted
        })
        .ToListAsync()
);

// Get by ID
app.MapGet("/api/todo/{id:int}", async (int id, ToDoDbContext db) =>
{
    var t = await db.ToDos.FindAsync(id);
    return t is null ? Results.NotFound() : Results.Ok(new ToDoDto
    {
        Id = t.Id,
        CreatedAt = t.CreatedAt,
        Title = t.Title,
        Description = t.Description,
        IsCompleted = t.IsCompleted
    });
});

// Create
app.MapPost("/api/todo", async (ToDoDto dto, ToDoDbContext db) =>
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

    return Results.Created($"/api/todo/{todo.Id}", new ToDoDto
    {
        Id = todo.Id,
        CreatedAt = dto.CreatedAt,
        Title = todo.Title,
        Description = todo.Description,
        IsCompleted = todo.IsCompleted
    });
});

// Update
app.MapPut("/api/todo/{id:int}", async (int id, ToDoDto dto, ToDoDbContext db) =>
{
    var t = await db.ToDos.FindAsync(id);
    if (t is null) return Results.NotFound();
    
    t.Title = dto.Title;
    t.Description = dto.Description;
    t.IsCompleted = dto.IsCompleted;

    await db.SaveChangesAsync();
    return Results.Ok(new ToDoDto
    {
        Id = t.Id,
        CreatedAt = t.CreatedAt,
        Title = t.Title,
        Description = t.Description,
        IsCompleted = t.IsCompleted
    });
});

// Delete
app.MapDelete("/api/todo/{id:int}", async (int id, ToDoDbContext db) =>
{
    var t = await db.ToDos.FindAsync(id);
    if (t is null) return Results.NotFound();

    db.ToDos.Remove(t);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();