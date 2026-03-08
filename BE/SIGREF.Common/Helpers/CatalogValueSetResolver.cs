using SIGREF.Common.Constants;
using SIGREF.Common.Types;

namespace SIGREF.Common.Helpers;

public static class CatalogValueSetResolver
{
    public static string Resolve(CatalogType type)
    {
        return type switch
        {
            CatalogType.Roles =>
                FhirNamespaces.RolesAdminValueSet,

            CatalogType.Ubicaciones =>
                FhirNamespaces.TiposUbicacionValueSet,

            _ => throw new ArgumentOutOfRangeException(
                nameof(type),
                $"Catálogo no soportado: {type}")
        };
    }
}