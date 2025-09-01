
# Unity-MCP Common

## Maintenance

### 1. Build package

```bash
dotnet pack -c Release
```

### 2. Get package

Get it from the location

```txt
\Unity-MCP-Plugin\Temp\bin\Release\com.IvanMurzak.Unity.MCP.Common.1.0.5.nupkg
```

## Variables

| Environment Variable        | Command Line Variable | Description                                                                 |
|-----------------------------|-----------------------|-----------------------------------------------------------------------------|
| `UNITY_MCP_PLUGIN_PORT`     | `--plugin-port`       | **Plugin** -> **Server** connection port (default: 60606)                   |
| `UNITY_MCP_PLUGIN_TIMEOUT`  | `--plugin-timeout`    | **Plugin** -> **Server** connection timeout (ms) (default: 10000)           |
| `UNITY_MCP_CLIENT_PORT`     | `--client-port`       | **Client** -> **Server** connection port (default: 80)                      |
| `UNITY_MCP_CLIENT_TRANSPORT`| `--client-transport`  | **Client** -> **Server** transport type: `stdio` or `http` (default: `http`) |
