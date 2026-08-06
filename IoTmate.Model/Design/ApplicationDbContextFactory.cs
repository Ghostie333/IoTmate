using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IoTmate.Model
{
    // Used only in design time (dotnet ef migrations). Creates the DbContext without
    // booting the web host, avoiding host-discovery failures during migrations.
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                                   ?? BuildConnectionString();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(connectionString)
                .Options;

            return new ApplicationDbContext(options);
        }

        private static string BuildConnectionString()
        {
            var host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
            var port = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
            var db = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "iotmate_db";
            var user = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "user";
            var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "user";

            return $"Host={host};Port={port};Database={db};Username={user};Password={password}";
        }
    }
}