using Microsoft.Extensions.FileProviders;

namespace SIGREF.API;

public partial class Startup
{
    private void UseMediaStaticFiles(IApplicationBuilder app, IWebHostEnvironment env)
    {
        var mediaPath = Path.Combine(env.ContentRootPath, "media");
        if (!Directory.Exists(mediaPath))
            Directory.CreateDirectory(mediaPath);

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(mediaPath),
            RequestPath = "/media"
        });
    }
}
