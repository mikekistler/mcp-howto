using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace ResourceLinks.Resources;

static class ResourceGenerator
{
    private static List<Resource> _resources = [];

    public static Resource? GetResource(int id) {
        return _resources.First(r => r.Name == $"Resource {id}");
    }

    public static Resource CreateResource(int id)
    {
        string uri = $"test://template/resource/{id}";
        string name = $"Resource {id}";
        Resource resource;

        if (id % 2 != 0)
        {
            resource = new Resource
            {
                Uri = uri,
                Name = name,
                MimeType = "text/plain",
                Description = $"Resource {id}: This is a plaintext resource"
            };
        }
        else
        {
            resource = new Resource
            {
                Uri = uri,
                Name = name,
                MimeType = "application/octet-stream",
                Description = $"Resource {id}: This is a base64 blob"
            };
        }

        _resources.Add(resource);
        return resource;
    }
}

[McpServerResourceType]
public class ResourceType
{
    [McpServerResource(UriTemplate = "test://template/resource/{id}", Name = "Template Resource")]
    [Description("A template resource with a numeric ID")]
    public static ResourceContents TemplateResource(RequestContext<ReadResourceRequestParams> requestContext, int id)
    {
        Resource? resource = ResourceGenerator.GetResource(id);
        if (resource is null)
        {
            throw new NotSupportedException($"Unknown resource: {requestContext.Params?.Uri}");
        }

        return resource.MimeType == "text/plain" ?
            new TextResourceContents
            {
                Uri = resource.Uri,
                MimeType = resource.MimeType,
                Text = resource.Description!,
            } :
            new BlobResourceContents
            {
                Uri = resource.Uri,
                MimeType = resource.MimeType,
                Blob = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(resource.Description!)),
            };
    }
}
