## Resource Links in Tool Results

Tools can now include **resource links** in their results, enabling better resource discovery and navigation.
This is particularly useful for tools that create or manage resources, allowing clients to easily access and interact with those resources.

In the example project, a tool creates a resource with a random value and returns a link to this resource:

```csharp
[McpServerTool]
[Description("Creates a resource with a random value and returns a link to this resource.")]
public async Task<CallToolResult> MakeAResource()
{
    int id = new Random().Next(1, 101); // 1 to 100 inclusive

    var resource = ResourceGenerator.CreateResource(id);

    var result = new CallToolResult();

    result.Content.Add(new ResourceLinkBlock()
    {
        Uri = resource.Uri,
        Name = resource.Name
    });

    return result;
}
```