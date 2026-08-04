using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace KindergartenApp.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<KindergartenDbContext>
{
    public KindergartenDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<KindergartenDbContext>();
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dbPath = System.IO.Path.Combine(appDataPath, "KindergartenApp");
        System.IO.Directory.CreateDirectory(dbPath);
        var connectionString = $"Data Source={System.IO.Path.Combine(dbPath, "kindergarten.db")}";
        
        optionsBuilder.UseSqlite(connectionString);
        return new KindergartenDbContext(optionsBuilder.Options);
    }
}
