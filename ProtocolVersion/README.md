## Protocol Version in Streamable HTTP Header

The protocol version for an MCP session is negotiated in the Initialization phase,
where the client and server exchange their supported protocol versions.
The highest mutually supported version is then selected for the session.
Protocol version negotiation has existed in MCP since the first public specification.

Starting with the 2025-06-18 version, the client MUST include the negotiated protocol version
in an `MCP-Protocol-Version` HTTP header on all subsequent requests to the MCP server when using the streamable HTTP transport.

### Client behavior

The C# SDK saves the negotiated protocol version and automatically includes it in the HTTP headers for all requests.

### Server behavior

The MCP C# SDK server logic does not fail requests that do not include the `MCP-Protocol-Version` header or
requests that include a protocol version that is different from the negotiated version
or requests that include a protocol version that is not supported by the server.

The server also does not (currently) exclude fields of responses that are not supported by the negotiated protocol version.
For example, the respose of `tools/list` will always include the `outputSchema` field for a tool that supports structured output,
even if the negotiated protocol version is prior to 2025-06-18 when this field was introduced.
