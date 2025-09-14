## HTTP Context

When using the Streamable HTTP transport, an MCP server may need to access the underlying [HttpContext] for a request.
The [HttpContext] contains request metadata such as the HTTP headers, authorization context, and the actual path and query string for the request.

To access the [HttpContext], the MCP server should add the [IHttpContextAccessor] service to the application service collection (typically in Program.cs).
Then any classes, e.g. a class containing MCP tools, should accept an [IHttpContextAccessor] in their constructor and store this for use by its methods.
Methods then use the [HttpContext property][IHttpContextAccessor.HttpContext] of the accessor to get the current context.

[HttpContext]: https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.http.httpcontext
[IHttpContextAccessor]: https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.http.ihttpcontextaccessor
[IHttpContextAccessor.HttpContext]: https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.http.ihttpcontextaccessor.httpcontext

The following code snippet illustrates how to add the [IHttpContextAccessor] service to the application service collection:

```csharp
builder.Services.AddHttpContextAccessor();
```

If `ContextTools` is a class whose methods need access to the [HttpContext], the following snippet shows how it can accept
an [IHttpContextAccessor] in its constructor and store it for later use:

```csharp
public class ContextTools
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ContextTools(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // remainder of ContextTools follows
```

A method of `ContextTools` can then access the current [HttpContext] as follows:

<!-- highlight the last 5 lines -->
```csharp
    [McpServerTool(UseStructuredContent = true)]
    [Description("Retrieves the HTTP headers from the current request and returns them as a JSON object.")]
    public object GetHttpHeaders()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            return "No HTTP context available";
        }
```

### Complete example

A complete example of an MCP Server that uses this method to access the [HttpContext] within tool methods is available
in [this sample project](https://github.com/mikekistler/mcp-howto/tree/main/HttpContext).

You can run the project and then use the [TryItOut.ipynb](./TryItOut.ipynb) notebook to call one of the tools
that access the HTTP context and returns the HTTP headers as a JSON object.
