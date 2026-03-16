using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SIGREF.Infrastructure.Persistence;

public class SIGREFContextFactory : IDesignTimeDbContextFactory<SIGREFContext>
{
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    /// 
    public SIGREFContext CreateDbContext(string[] args)
    {
        //dotnet ef migrations add UpdateReportHistory `
       // --project SIGREF.Infrastructure.Persistence\SIGREF.Infrastructure.Persistence.csproj `
       // --startup-project SIGREF.Infrastructure.Persistence\SIGREF.Infrastructure.Persistence.csproj `
       // --context SIGREF.Infrastructure.Persistence.SIGREFContext `
       // --output-dir Migrations
        var optionsBuilder = new DbContextOptionsBuilder<SIGREFContext>();
        
        // Usamos una cadena de conexión ficticia o local solo para diseño.
        optionsBuilder.UseNpgsql("Host=localhost;Database=sigref_db;Username=postgres;Password=postgres");

        return new SIGREFContext(optionsBuilder.Options);
    }
}