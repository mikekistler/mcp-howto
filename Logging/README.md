## Logging

MCP servers may expose log messages to clients through the [Logging utility].

[Logging utility]: https://modelcontextprotocol.io/specification/2025-06-18/server/utilities/logging

### Server configuration and logging

MCP servers that implement the Logging utility must declare this in the capabilities send in the
[Initialization] phase at the beginning of the MCP session.

[Initialization]: https://modelcontextprotocol.io/specification/2025-06-18/basic/lifecycle#initialization

Servers built with the C# SDK always declare the logging capability.

Support for logging does not require any specific configuration on the server side.

### Logging Levels

MCP uses the logging levels defined in [RFC 5424](https://tools.ietf.org/html/rfc5424). Here's the levels and how they map to standard ASP.NET Core logging levels.

| Level | Description | Example Use Case |
|-------|-------------|------------------|
| debug | Detailed debugging information | Function entry/exit points |
| info | General informational messages | Operation progress updates |
| notice | Normal but significant events | Configuration changes |
| warning | Warning conditions | Deprecated feature usage |
| error | Error conditions | Operation failures |
| critical | Critical conditions | System component failures |
| alert | Action must be taken immediately | Data corruption detected |
| emergency | System is unusable | |


