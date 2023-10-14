using Microsoft.EntityFrameworkCore;
using BlogApp.Entity;

namespace BlogApp.Data.Concrete.EfCore
{
    public class BlogContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.Development.json");

            var configuration = builder.Build();

            optionsBuilder.UseSqlServer(connectionString: configuration["ConnectionStrings:sql_connection"]);
        }

        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
