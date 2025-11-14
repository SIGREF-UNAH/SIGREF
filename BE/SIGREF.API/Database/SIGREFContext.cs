using Microsoft.EntityFrameworkCore;
using SIGREF.API.Database.Entity;

namespace SIGREF.API.Database;
    public class SIGREFContext : DbContext
    {
        public SIGREFContext(DbContextOptions<SIGREFContext> options) : base(options)
        {
        }

        //DbSets 
        public DbSet<UserLink> UserLinks { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aplica automaticamente todas las configuraciones
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SIGREFContext).Assembly);
        }
    }
