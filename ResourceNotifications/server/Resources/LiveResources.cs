using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace ResourceNotifications.Resources;

static class ResourceManager
{
    private static List<TextResourceContents> _resources = [];

    // Subscriptions tracks resource URIs to McpServer instances
    public static Dictionary<string, IMcpServer> Subscriptions = [];

    public static IReadOnlyList<TextResourceContents> ListResources()
    {
        return _resources.AsReadOnly();
    }

    public static TextResourceContents? GetResource(int id)
    {
        var resource = _resources.FirstOrDefault(r => r.Uri == $"test://resource/{id}");
        if (resource is null)
        {
            resource = ResourceManager.CreateResource(id);
        }
        return resource;
    }

    public static TextResourceContents CreateResource(int id)
    {
        string uri = $"test://resource/{id}";
        string name = $"Resource {id}";
        var resource = new TextResourceContents
        {
            Uri = uri,
            MimeType = "text/plain",
            Text = $"Created at {DateTime.UtcNow:O}"
        };

        _resources.Add(resource);
        return resource;
    }
}

[McpServerResourceType]
public class LiveResources
{
    [McpServerResource(UriTemplate = "test://resource/{id}", Name = "Live Resource")]
    [Description("A live resource with a numeric ID")]
    public static ResourceContents TemplateResource(RequestContext<ReadResourceRequestParams> requestContext, int id)
    {
        TextResourceContents? resource = ResourceManager.GetResource(id);
        if (resource is null)
        {
            throw new NotSupportedException($"Unknown resource: {requestContext.Params?.Uri}");
        }

        return resource;
    }
}
