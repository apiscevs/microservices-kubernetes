using CommandService.Models;
using Microsoft.EntityFrameworkCore;

namespace CommandService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        
        public DbSet<Platform> Platforms { get; set; }
        public DbSet<Command> Commands { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Platform>()
                .HasMany(t => t.Commands)
                .WithOne(t => t.Platform)
                .HasForeignKey(t => t.PlatformId);
            
            // CosmosDb specific, configure the container name and partition key
            modelBuilder.Entity<Platform>()
                .ToContainer("Platforms") 
                .HasPartitionKey(p => p.ExternalId); 

            modelBuilder.Entity<Command>()
                .ToContainer("Commands")
                .HasPartitionKey(c => c.PlatformId);
            
            base.OnModelCreating(modelBuilder);
        }
    }
}
