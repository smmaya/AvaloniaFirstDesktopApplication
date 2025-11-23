using Avalonia.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Avalonia.Api.Data;

public class ToDoDbContext(DbContextOptions<ToDoDbContext> options) : DbContext(options)
{
    public DbSet<ToDo> ToDos => Set<ToDo>();
}