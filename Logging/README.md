## Logging

MCP servers may expose log messages to clients through the [Logging utility].

[Logging utility]: https://modelcontextprotocol.io/specification/2025-06-18/server/utilities/logging

### Server configuration and logging

MCP servers that implement the Logging utility must declare this in the capabilities send in the
[Initialization] phase at the beginning of the MCP session.

[Initialization]: https://modelcontextprotocol.io/specification/2025-06-18/basic/lifecycle#initialization

In the C# SDK,
