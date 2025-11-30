using Hl7.Fhir.Model;

namespace SIGREF.API.Extensions.Common;

public static class BundleResolver
{
    public static T Resolve<T>(this Bundle.EntryComponent bundle, ResourceReference reference)
        where T : Resource
    {
        if (reference == null || string.IsNullOrWhiteSpace(reference.Reference))
            return null;

        var parts = reference.Reference.Split('/');
        if (parts.Length != 2)
            return null;

        var resourceType = parts[0];
        var id = parts[1];

        return bundle.Resource is T resource && resource.TypeName == resourceType && resource.Id == id
            ? resource
            : null;
    }
}