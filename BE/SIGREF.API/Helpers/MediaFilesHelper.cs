using SIGREF.API.Database.Entity.common;

namespace SIGREF.API.Helpers;

public static class MediaHelper
{
    public static readonly string[] AllowedImageExtensions =
    {
        ".jpg", ".jpeg", ".png"
    };
}

public static class MediaPathHelper
{
    public static string GetFolder(MediaFileType type)
        => type switch
        {
            MediaFileType.AppHospital => "logo_hospital",
            MediaFileType.HealthGuilt => "health_guilt",
            _ => throw new Exception("Tipo de archivo inválido")
        };
}