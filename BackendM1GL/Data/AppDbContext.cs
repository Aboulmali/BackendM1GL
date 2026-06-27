using BackendM1GL.Entities;
using Microsoft.EntityFrameworkCore;

namespace BackendM1GL.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Tes tables (DbSet = une table par entité)
        public DbSet<User> Users => Set<User>();
        public DbSet<Product> Products { get; set; }
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        // Configuration avancée (optionnel)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Charge automatiquement toutes les configurations dans /Data/Configurations
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}

