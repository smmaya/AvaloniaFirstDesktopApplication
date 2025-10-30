using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Avalonia.Api.Data;

public class ToDoDbContextFactory : IDesignTimeDbContextFactory<ToDoDbContext>
{
    public ToDoDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ToDoDbContext>();
        optionsBuilder.UseSqlite("Data Source=todo.db");

        return new ToDoDbContext(optionsBuilder.Options);
    }
}