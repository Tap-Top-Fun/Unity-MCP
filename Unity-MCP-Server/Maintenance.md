<div align="center">
  <h1>Unity MCP server maintenance</h1>

[![Docker Image](https://img.shields.io/docker/image-size/ivanmurzakdev/unity-mcp-server/latest?label=Docker%20Image&logo=docker&labelColor=333A41 'Docker Image')](https://hub.docker.com/r/ivanmurzakdev/unity-mcp-server)
[![MCP](https://badge.mcpx.dev?type=server 'MCP Server')](https://modelcontextprotocol.io/introduction)
[![r](https://github.com/IvanMurzak/Unity-MCP/workflows/release/badge.svg 'Tests Passed')](https://github.com/IvanMurzak/Unity-MCP/actions/workflows/release.yml)
[![Unity Asset Store](https://img.shields.io/badge/Asset%20Store-View-blue?logo=unity&labelColor=333A41 'Asset Store')](https://u3d.as/3wsw)
[![Unity Editor](https://img.shields.io/badge/Editor-X?style=flat&logo=unity&labelColor=333A41&color=49BC5C 'Unity Editor supported')](https://unity.com/releases/editor/archive)
[![Unity Runtime](https://img.shields.io/badge/Runtime-X?style=flat&logo=unity&labelColor=333A41&color=49BC5C 'Unity Runtime supported')](https://unity.com/releases/editor/archive)
[![OpenUPM](https://img.shields.io/npm/v/com.ivanmurzak.unity.mcp?label=OpenUPM&registry_uri=https://package.openupm.com&labelColor=333A41 'OpenUPM package')](https://openupm.com/packages/com.ivanmurzak.unity.mcp/)</br>
[![Stars](https://img.shields.io/github/stars/IvanMurzak/Unity-MCP 'Stars')](https://github.com/IvanMurzak/Unity-MCP/stargazers)
[![License](https://img.shields.io/github/license/IvanMurzak/Unity-MCP?label=License&labelColor=333A41)](https://github.com/IvanMurzak/Unity-MCP/blob/main/LICENSE)
[![Stand With Ukraine](https://raw.githubusercontent.com/vshymanskyy/StandWithUkraine/main/badges/StandWithUkraine.svg)](https://stand-with-ukraine.pp.ua)

</div>

# Create docker image

## 1. Build your image

From the folder with your Dockerfile:

```bash
docker build -t ivanmurzakdev/unity-mcp-server:latest .
```

## 2. Tag the image with your Docker Hub repo name

```bash
docker tag unity-mcp-server:latest ivanmurzakdev/unity-mcp-server:latest
```

## 3. Log in to Docker Hub

```bash
docker login
```

## 4. Push the image

```bash
docker push ivanmurzakdev/unity-mcp-server:latest
```

# Clean local NuGet cache

```bash
dotnet nuget locals all --clear
```
