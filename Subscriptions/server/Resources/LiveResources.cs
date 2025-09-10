using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Collections.Concurrent;

namespace Subscriptions.Resources;

static class ResourceManager
{
    private static ConcurrentQueue<TextResourceContents> _resources = new();

    // Subscriptions tracks resource URIs to bags of McpServer instances (thread-safe via locking)
    private static Dictionary<string, List<IMcpServer>> Subscriptions = new();

    private static Dictionary<string /* sessionId */, List<string> /* uris */> SessionSubscriptions = new();
    private static readonly object _subscriptionsLock = new();

    public static void AddSubscription(string uri, IMcpServer server)
    {
        lock (_subscriptionsLock)
        {
            if (!Subscriptions.TryGetValue(uri, out var list))
            {
                list = new List<IMcpServer>();
                Subscriptions[uri] = list;
            }
            list.Add(server);
            SessionSubscriptions[server.SessionId] ??= new List<string>();
            SessionSubscriptions[server.SessionId].Add(uri);
        }
    }

    public static void RemoveSubscription(string uri, IMcpServer server)
    {
        lock (_subscriptionsLock)
        {
            if (Subscriptions.TryGetValue(uri, out var list))
            {
                Subscriptions[uri] = list.Where(s => s.SessionId != server.SessionId).ToList();
            }
        }
    }

    public static List<IMcpServer> GetSubscriptions(string uri)
    {
        lock (_subscriptionsLock)
        {
            if (Subscriptions.TryGetValue(uri, out var list))
            {
                return list.ToList();
            }
            return new List<IMcpServer>();
        }
    }

    public static void RemoveAllSubscriptions(IMcpServer server)
    {
        lock (_subscriptionsLock)
        {
            var keys = Subscriptions.Keys.ToList();
            foreach (var uri in keys)
            {
                if (Subscriptions.TryGetValue(uri, out var list))
                {
                    Subscriptions[uri] = list.Where(s => s.SessionId != server.SessionId).ToList();
                }
            }
        }
    }

    public static IReadOnlyCollection<TextResourceContents> ListResources()
    {
        return _resources.ToArray();
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

        _resources.Enqueue(resource);
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
