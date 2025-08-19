# Unity MCP server

Model Context Protocol implementation for Unity Editor and for games made with Unity.

## Topology

- **Client** is the MCP client, such as VS Code, Cursor, Claude Desktop, Claude Code etc.
- **Server** is the MCP server, this is Unity-MCP server implementation which works closely in pair with Unity MCP Plugin
- **Plugin** is the Unity-MCP Plugin, this is deeply connected with Unity Editor and runtime game build SDK, that exposes API for the Server and lets the AI magic to happen. It utilizes advanced reflection by using [ReflectorNet](https://github.com/IvanMurzak/ReflectorNet)

### Connection chain

**Client** <-> **Server** <-> **Plugin** (Unity Editor / Game Build)

---

## Launch

Unity-MCP server is developed with idea of flexibility in mind, that is why it has many launch options.

**Command Line**

```bash
./unity-mcp-server
```

**Docker**

```bash
docker run -it --rm -p 80:80 -p 60606:60606 ivanmurzakdev/unity-mcp-server
```

### Variables

Doesn't matter what launch option you choose, all of them support custom configuration using both Environment Variables and Command Line Arguments. It would work with default values, if you just need to launch it, don't waste your time for the variables. Just make sure Unity Plugin also has default values, especially the `--plugin-port`, they should be equal.

| Environment Variable        | Command Line Args     | Description                                                                 |
|-----------------------------|-----------------------|-----------------------------------------------------------------------------|
| `UNITY_MCP_PLUGIN_PORT`     | `--plugin-port`       | **Plugin** -> **Server** connection port (default: 60606)                   |
| `UNITY_MCP_PLUGIN_TIMEOUT`  | `--plugin-timeout`    | **Plugin** -> **Server** connection timeout (ms) (default: 10000)           |
| `UNITY_MCP_CLIENT_PORT`     | `--client-port`       | **Client** -> **Server** connection port (default: 80)                      |
| `UNITY_MCP_CLIENT_TRANSPORT`| `--client-transport`  | **Client** -> **Server** transport type: `stdio` or `http` (default: `http`) |

## Quick launch

> If you don't need to deploy the `Unity-MCP Server` somewhere, and you want to use Unity Editor in pair with AI, you probably should be enough to use only `Unity-MCP Plugin` for Unity Editor. It installs relevant `Unity-MCP Server` automatically using Binary for your current operation system and CPU architecture.

### Using Docker (recommended)

```bash
docker run -it --rm -p 80:80 -p 60606:60606 ivanmurzakdev/unity-mcp-server
```

### Using Binary

Download binary from the [GitHub releases page](https://github.com/IvanMurzak/Unity-MCP/releases). Unpack the zip archive and use command line to simply launch binary of the server for your target operation system and CPU architecture.

## Launch STDIO (Local)

Launch server with STDIO transport type for local usage on the same machine with Unity Editor.

```bash
./unity-mcp-server --plugin-port 60606 --plugin-timeout 10000 --client-transport stdio
```

## Launch HTTP(S) (Local OR Remote)

Launch server with HTTP transport type for local OR remote usage using HTTP(S) url.

```bash
./unity-mcp-server --plugin-port 60606 --plugin-timeout 10000 --client-transport http
```

## Launch using Docker

```bash
docker run -it --rm -p 80:80 -p 60606:60606 ivanmurzakdev/unity-mcp-server
```

Use environment variables to customize settings

```bash
docker run -it --rm -e UNITY_MCP_PLUGIN_PORT=60606 -p 80:80 -p 60606:60606 ivanmurzakdev/unity-mcp-server
```


