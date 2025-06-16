using Microsoft.EntityFrameworkCore;
using MyAuthApp.Models;

namespace MyAuthApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }  // Represents the Users table
        public DbSet<Tweet> Tweets { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Optionally, override OnModelCreating for additional configuration
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Ensure Username and Email are unique
            builder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            builder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}
