using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Collections.Concurrent;

namespace Subscriptions.Resources;

static class ResourceManager
{
    private static ConcurrentDictionary<string, TextResourceContents> _resources = new();

    public static IReadOnlyCollection<TextResourceContents> ListResources()
    {
        return _resources.Values.ToArray();
    }

    public static TextResourceContents? GetResource(string uri)
    {
        // var resource =  _resources.TryGetValue(uri, out var res) ? res : null;
        if (!_resources.TryGetValue(uri, out var resource))
        {
            resource = ResourceManager.CreateResource(uri);
        }
        return resource;
    }

    public static TextResourceContents? CreateResource(string uri)
    {
        // Extract ID from URI
        var match = System.Text.RegularExpressions.Regex.Match(uri, @"test://resource/(\d+)");
        if (!match.Success || match.Groups.Count < 2 || !int.TryParse(match.Groups[1].Value, out int id))
        {
            return null;
        }
        string name = $"Resource {id}";
        var resource = new TextResourceContents
        {
            Uri = uri,
            MimeType = "text/plain",
            Text = $"Created at {DateTime.UtcNow:O}"
        };

        _resources.TryAdd(uri, resource);
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
        var uri = $"test://resource/{id}";
        TextResourceContents? resource = ResourceManager.GetResource(uri);
        if (resource is null)
        {
            throw new NotSupportedException($"Unknown resource: {requestContext.Params?.Uri}");
        }

        return resource;
    }
}
