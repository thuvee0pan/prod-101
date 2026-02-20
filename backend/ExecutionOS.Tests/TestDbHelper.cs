using ExecutionOS.API.Data;
using Microsoft.EntityFrameworkCore;

namespace ExecutionOS.Tests;

public static class TestDbHelper
{
    public static AppDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var db = new AppDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }
}
