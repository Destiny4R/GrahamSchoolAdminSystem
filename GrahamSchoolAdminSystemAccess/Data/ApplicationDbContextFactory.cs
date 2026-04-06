using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace GrahamSchoolAdminSystemAccess.Data
{
    /// <summary>
    /// DbContext factory for Entity Framework Core migrations
    /// This allows EF Core tools to create database contexts without needing the full dependency injection container
    /// </summary>
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        //private readonly IConfiguration _config;

        //public ApplicationDbContextFactory(IConfiguration config)
        //{
        //    _config = config;
        //}

        //public ApplicationDbContext CreateDbContext(string[] args)
        //{
        //    // Create DbContext options builder
        //    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        //    // Connection string from appsettings.json
        //    string connectionString = _config["ConnectionStrings:DefaultConnection"];

        //    optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

        //    return new ApplicationDbContext(optionsBuilder.Options);
        //}
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // 2. Build your configuration manually inside the method
            IConfigurationRoot configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // 3. Use your database provider (ensure the NuGet package is installed)
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            return new ApplicationDbContext(optionsBuilder.Options);
        }
        
    }
}
