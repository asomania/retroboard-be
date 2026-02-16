using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Retroboard.Api.Infrastructure.Data;

public class RetroboardDesignTimeDbContextFactory : IDesignTimeDbContextFactory<RetroboardDbContext>
{
    public RetroboardDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("DefaultConnection is missing.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<RetroboardDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new RetroboardDbContext(optionsBuilder.Options);
    }
}
