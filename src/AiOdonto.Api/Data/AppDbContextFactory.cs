using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AiOdonto.Api.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("Server=localhost\\SQLEXPRESS_2022;Database=AiOdonto;Trusted_Connection=true;TrustServerCertificate=true")
            .Options;
        return new AppDbContext(options);
    }
}
