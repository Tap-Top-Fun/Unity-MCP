# ✨ AI Game Developer — *Unity MCP*

[![MCP](https://badge.mcpx.dev?type=server 'MCP Server')](https://modelcontextprotocol.io/introduction) [![Docker Image](https://img.shields.io/docker/image-size/ivanmurzakdev/unity-mcp-server/latest?label=Docker%20Image&logo=docker&labelColor=333A41 'Docker Image')](https://hub.docker.com/r/ivanmurzakdev/unity-mcp-server)
[![Unity Asset Store](https://img.shields.io/badge/Asset%20Store-View-blue?logo=unity&labelColor=333A41 'Asset Store')](https://u3d.as/3wsw) [![Unity Editor](https://img.shields.io/badge/Editor-X?style=flat&logo=unity&labelColor=333A41&color=49BC5C 'Unity Editor supported')](https://unity.com/releases/editor/archive) [![Unity Runtime](https://img.shields.io/badge/Runtime-X?style=flat&logo=unity&labelColor=333A41&color=49BC5C 'Unity Runtime supported')](https://unity.com/releases/editor/archive) [![OpenUPM](https://img.shields.io/npm/v/com.ivanmurzak.unity.mcp?label=OpenUPM&registry_uri=https://package.openupm.com&labelColor=333A41 'OpenUPM package')](https://openupm.com/packages/com.ivanmurzak.unity.mcp/)
[![Stars](https://img.shields.io/github/stars/IvanMurzak/Unity-MCP 'Stars')](https://github.com/IvanMurzak/Unity-MCP/stargazers) [![r](https://github.com/IvanMurzak/Unity-MCP/workflows/release/badge.svg 'Tests Passed')](https://github.com/IvanMurzak/Unity-MCP/actions/workflows/release.yml) [![License](https://img.shields.io/github/license/IvanMurzak/Unity-MCP?label=License&labelColor=333A41)](https://github.com/IvanMurzak/Unity-MCP/blob/main/LICENSE) [![Stand With Ukraine](https://raw.githubusercontent.com/vshymanskyy/StandWithUkraine/main/badges/StandWithUkraine.svg)](https://stand-with-ukraine.pp.ua)

AI helper which does wide range of tasks in Unity Editor and even in a running game compiled to any platform. It connects to AI using TCP connection, that is why it is so flexible.

## Features for a human

- ✅ Few clicks installation
- ✅ Chat with AI like with a human
- ✅ Wide range of default [AI tools](https://github.com/IvanMurzak/Unity-MCP/blob/main/docs/ai-tools.md)
- ✅ Use `Description` attribute in C# code to provide detailed information for `class`, `field`, `property` or `method`.
- ✅ Customizable reflection convertors, inspired by `System.Text.Json` convertors
  - do you have something extremely custom in your project? Make custom reflection convertor to let LLM be able to read and write into that data
- ✅ Remote AI units setup using docker containers,
  - make a team of AI workers which work on your project simultaneously

## Features for LLM

- ✅ Agent ready tools, find anything you need in 1-2 steps
- ✅ Instant C# code compilation & execution using `Roslyn`, iterate faster
- ✅ Assets access (read / write), C# scripts access (read / write)
- ✅ Well described positive and negative feedback for proper understanding of an issue
- ✅ Provide references to existed objects for the instant C# code using `Reflection`
- ✅ Get full access to entire project data in a readable shape using `Reflection`
- ✅ Populate & Modify any granular piece of data in the project using `Reflection`
- ✅ Find any `method` in the entire codebase, including compiled DLL files using `Reflection`
- ✅ Call any `method` in the entire codebase using `Reflection`
- ✅ Provide any property into `method` call, even if it is a reference to existed object in memory using `Reflection` and advanced reflection convertors
- ✅ Unity API instantly available for usage, even if Unity changes something you will get fresh API using `Reflection`.
- ✅ Get access to human readable description of any `class`, `method`, `field`, `property` by reading it's `Description` attribute.

### Stability status

| Unity Version | Editmode | Playmode | Standalone |
|---------------|----------|----------|------------|
| 2022.3.61f1   | [![r](https://github.com/IvanMurzak/Unity-MCP/workflows/release/badge.svg?job=test-unity-2022-3-61f1-editmode)](https://github.com/IvanMurzak/Unity-MCP/actions/workflows/release.yml) | [![r](https://github.com/IvanMurzak/Unity-MCP/workflows/release/badge.svg?job=test-unity-2022-3-61f1-playmode)](https://github.com/IvanMurzak/Unity-MCP/actions/workflows/release.yml) | [![r](https://github.com/IvanMurzak/Unity-MCP/workflows/release/badge.svg?job=test-unity-2022-3-61f1-standalone)](https://github.com/IvanMurzak/Unity-MCP/actions/workflows/release.yml)
| 2023.2.20f1   | [![r](https://github.com/IvanMurzak/Unity-MCP/workflows/release/badge.svg?job=test-unity-2023-2-20f1-editmode)](https://github.com/IvanMurzak/Unity-MCP/actions/workflows/release.yml) | [![r](https://github.com/IvanMurzak/Unity-MCP/workflows/release/badge.svg?job=test-unity-2023-2-20f1-playmode)](https://github.com/IvanMurzak/Unity-MCP/actions/workflows/release.yml) | [![r](https://github.com/IvanMurzak/Unity-MCP/workflows/release/badge.svg?job=test-unity-2023-2-20f1-standalone)](https://github.com/IvanMurzak/Unity-MCP/actions/workflows/release.yml)
| 6000.2.3f1   | [![r](https://github.com/IvanMurzak/Unity-MCP/workflows/release/badge.svg?job=test-unity-6000-2-3f1-editmode)](https://github.com/IvanMurzak/Unity-MCP/actions/workflows/release.yml) | [![r](https://github.com/IvanMurzak/Unity-MCP/workflows/release/badge.svg?job=test-unity-6000-2-3f1-playmode)](https://github.com/IvanMurzak/Unity-MCP/actions/workflows/release.yml) | [![r](https://github.com/IvanMurzak/Unity-MCP/workflows/release/badge.svg?job=test-unity-6000-2-3f1-standalone)](https://github.com/IvanMurzak/Unity-MCP/actions/workflows/release.yml)

## Requirements

> [!IMPORTANT]
> **Project path cannot contain spaces**
>
> - ✅ `C:/MyProjects/Project`
> - ❌ `C:/My Projects/Project`

### Install MCP client

Choose MCP client you prefer, don't need to install all of them. This is will be your main chat window to talk with LLM.

- [GitHub Copilot in VS Code](https://code.visualstudio.com/docs/copilot/overview)
- [Cursor](https://www.cursor.com/)
- [Claude Desktop](https://claude.ai/download)
- [Claude Code](https://github.com/anthropics/claude-code)
- [Windsurf](https://windsurf.com)
- Any other supported

> MCP protocol is quite universal, that is why you may any MCP client you prefer, it will work as smooth as anyone else. The only important thing, that the MCP client has to support dynamic tool update.

# 👉 Installation

## Option 1: Install `.unitypackage` installer

-  **[⬇️ Download the Installer ⬇️](https://github.com/IvanMurzak/Unity-MCP/releases/download/0.16.2/AI-Game-Dev-Installer.unitypackage)**
- **📂 Open the installer into Unity project 📂**
  > - You may use double click on the file - Unity will open it
  > - OR: You may open Unity Editor first, then click on `Assets/Import Package/Custom Package`, then choose the file

## Option 2: Using OpenUPM

> This option is recommended for people well familiar with command line and probably with the NPM package managers.

- [Install OpenUPM-CLI](https://github.com/openupm/openupm-cli#installation)
- Open command line in Unity project folder
- Run the command

```bash
openupm add com.ivanmurzak.unity.mcp
```

# 👉 Configure MCP client

## 🟢 Step 1 Open `AI Connector` window

Open Unity project, go 👉 `Window/AI Connector (Unity-MCP)`.

![Unity_AI](https://github.com/IvanMurzak/Unity-MCP/blob/main/docs/img/ai-connector-window.gif)

## 🟢 Step 2: Click `Configure` at your MCP client

  > If MCP client is not in the list, use the raw JSON below in the window, to inject it into your MCP client. Read instructions for your MCP client how to do that.

# 👉 Talk to LLM

> Make sure `Agent` mode is turned on in MCP client

  ```text
  Explain my scene hierarchy
  ```

  ```text
  Create 3 cubes in a circle with radius 2
  ```

  ```text
  Create metallic golden material and attach it to a sphere gameObject
  ```

---

# How it works

**[Unity-MCP](https://github.com/IvanMurzak/Unity-MCP)** is a bridge between LLM and Unity. It exposes and explains to LLM Unity's tools. LLM understands the interface and utilizes the tools in the way a user asks.

Connect **[Unity-MCP](https://github.com/IvanMurzak/Unity-MCP)** to LLM client such as [Claude](https://claude.ai/download) or [Cursor](https://www.cursor.com/) using integrated `AI Connector` window. Custom clients are supported as well.

The project is designed to let developers to add custom tools soon. After that the next goal is to enable the same features in player's build. For not it works only in Unity Editor.

The system is extensible: you can define custom `tool`s directly in your Unity project codebase, exposing new capabilities to the AI or automation clients. This makes Unity-MCP a flexible foundation for building advanced workflows, rapid prototyping, or integrating AI-driven features into your development process.

---

# Advanced MCP server setup

Unity-MCP server supports many different launch options and docker docker deployment. Both transport protocol are supported `http` and `stdio`. [Read more...](https://github.com/IvanMurzak/Unity-MCP/blob/main/docs/mcp-server.md)

# Add custom `tool`

> ⚠️ It only works with MCP client that supports dynamic tool list update.

Unity-MCP is designed to support custom `tool` development by project owner. MCP server takes data from Unity plugin and exposes it to a Client. So anyone in the MCP communication chain would receive the information about a new `tool`. Which LLM may decide to call at some point.

To add a custom `tool` you need:

1. To have a class with attribute `McpPluginToolType`.
2. To have a method in the class with attribute `McpPluginTool`.
3. [optional] Add `Description` attribute to each method argument to let LLM to understand it.
4. [optional] Use `string? optional = null` properties with `?` and default value to mark them as `optional` for LLM.

> Take a look that the line `MainThread.Instance.Run(() =>` it allows to run the code in Main thread which is needed to interact with Unity API. If you don't need it and running the tool in background thread is fine for the tool, don't use Main thread for efficiency purpose.

```csharp
[McpPluginToolType]
public class Tool_GameObject
{
    [McpPluginTool
    (
        "MyCustomTask",
        Title = "Create a new GameObject"
    )]
    [Description("Explain here to LLM what is this, when it should be called.")]
    public string CustomTask
    (
        [Description("Explain to LLM what is this.")]
        string inputData
    )
    {
        // do anything in background thread

        return MainThread.Instance.Run(() =>
        {
            // do something in main thread if needed

            return $"[Success] Operation completed.";
        });
    }
}
```

# Add custom in-game `tool`

> ⚠️ Not yet supported. The work is in progress

# Running PlayMode tests

To be able to run Play Mode tests via the TestRunner MCP tool, you should consider configuring Unity to **not** perform a domain reload when entering Play Mode (`Edit -> Project Settings -> Editor -> Enter Play Mode Settings` - set to `Reload Scene only` or `Do not reload Domain or Scene`). Otherwise, starting the Play Mode tests will interrupt the TestRunner MCP tool, leading to a cycle of tests restarting.

---

# Contribution 💙💛

Contribution is highly appreciated. Brings your ideas and lets make the game development as simple as never before! Do you have an idea of a new `tool`, feature or did you spot a bug and know how to fix it.

1. 👉 [Fork the project](https://github.com/IvanMurzak/Unity-MCP/fork)
2. Clone the fork and open the `./Unity-MCP-Plugin` folder in Unity
3. Implement new things in the project, commit, push it to GitHub
4. Create Pull Request targeting original [Unity-MCP](https://github.com/IvanMurzak/Unity-MCP) repository, `main` branch.
