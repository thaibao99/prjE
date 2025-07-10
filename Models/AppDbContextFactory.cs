using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace prjetax.Models
{
    // EF migrations sẽ gọi factory này để khởi tạo AppDbContext
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Build config từ appsettings.json
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())           // thư mục project root
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            var connStr = config.GetConnectionString("DefaultConnection");
            optionsBuilder.UseSqlServer(connStr);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
