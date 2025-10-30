using Avalonia.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Avalonia.Api.Data;

public class ToDoDbContext : DbContext
{
    public ToDoDbContext(DbContextOptions<ToDoDbContext> options) : base(options)
    {
    }

    public DbSet<ToDo> ToDos => Set<ToDo>();
}