using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SIGREF.API.Database
{
    public class SIGREFContext : IdentityDbContext<IdentityUser>
    {
        public SIGREFContext(DbContextOptions options) : base(options)
        {
            
        }
    }
}
