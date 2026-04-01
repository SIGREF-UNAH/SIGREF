namespace SIGREF.API;

public partial class Startup
{
    private void AddCorsPolicy(IServiceCollection services)
    {
        services.AddCors(opt =>
        {
            {
                var allowURLS = _configuration.GetSection("AllowURLS").Get<string[]>();
                opt.AddPolicy("CorsPolicy", builder => builder
                    .WithOrigins(allowURLS)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .SetPreflightMaxAge(TimeSpan.FromMinutes(10)));
            }
        });

    }

}