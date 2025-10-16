using Microsoft.EntityFrameworkCore;

namespace SIGREF.API.Database
{
    public class SIGREFContext : DbContext
    {
        public SIGREFContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<AuditLog> AuditLogs { get; set; }
    }
}
