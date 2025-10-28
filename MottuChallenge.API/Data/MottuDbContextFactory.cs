using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace MottuChallenge.API.Data
{
    public class MottuDbContextFactory : IDesignTimeDbContextFactory<MottuDbContext>
    {
        public MottuDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<MottuDbContext>();
            var connectionString = configuration.GetConnectionString("OracleConnection");

            optionsBuilder.UseOracle(connectionString, options =>
            {
                options.MigrationsAssembly("MottuChallenge.API");
                options.CommandTimeout(60); 
            });

            return new MottuDbContext(optionsBuilder.Options);
        }
    }
}