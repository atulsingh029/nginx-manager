using Microsoft.EntityFrameworkCore;

namespace nginx_manager.Models
{
    // Simple example entity
    public class Host
    {
        public int Id { get; set; }
        public string Hostname { get; set; } = string.Empty;
        public string Ip { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; } 
    }

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Host> Hosts => Set<Host>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Host>(entity =>
            {
                entity.ToTable("hosts");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Hostname).HasMaxLength(255).IsRequired();
                entity.Property(e => e.Ip).HasMaxLength(64).IsRequired();
            
            });
        }
    }
}