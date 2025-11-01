# Pixel Perfect Console

[![Unity Version](https://img.shields.io/badge/Unity-2021.3%2B-blueviolet)](https://unity.com/)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

<img width="953" height="536" alt="image" src="https://github.com/user-attachments/assets/f3070e20-d369-4b84-8b7a-a1e9d9136218" />


A lightweight, extensible in-game debug console for Unity that allows developers to expose methods as console commands using simple attributes. Perfect for debugging, testing, and creating developer tools in your Unity projects.

## Features

- **Attribute-Based Command System** - Mark any method with `[Command("CommandName")]` to instantly make it accessible from the console
- **Build Stripping Support** - Keep sensitive developer commands out of production builds with the `strip` parameter
- **Type-Safe Parameters** - Automatic type conversion for common types (int, float, bool, string, enum, etc.)
- **Command History** - Navigate previous commands with up/down arrow keys
- **Clean UI** - Built with TextMeshPro for crisp, scalable text rendering
- **Auto-Discovery** - Automatically finds and registers commands from MonoBehaviours and static methods
- **Static & Instance Methods** - Support for both static commands and instance-based commands
- **Built-in Help System** - `help` command lists all available commands with descriptions
- **Zero Setup** - Add prefab to scene and start creating commands immediately

## 📦 Installation

### Option 1: Unity Package Manager (Git URL)

1. Open Unity Package Manager (`Window > Package Manager`)
2. Click the `+` button in the top-left corner
3. Select `Add package from git URL...`
4. Enter: `https://github.com/16Byte/PixelPerfectConsole.git`
   <img width="646" height="310" alt="image" src="https://github.com/user-attachments/assets/de124113-f0bc-4e5e-9e44-70d0b08dbd45" />


### Option 2: Manual Installation

1. Download or clone this repository
2. Copy the root folder into your project's `Assets` directory

## 🚀 Quick Start

### 1. Add Console to Scene

Drag the `ConsoleUI` prefab from `Samples/Prefabs/ConsoleUI.prefab` into your scene. That's it! The console is ready to use.

### 2. Toggle Console

Press **`~` (tilde)** or **F1** to open/close the console in Play mode.

### 3. Create Your First Command

```csharp
using UnityEngine;
using PPConsole;

public class GameManager : MonoBehaviour
{
    private int playerHealth = 100;

    [Command("GetHealth", Description = "Shows current player health")]
    private void GetHealth()
    {
        ConsoleManager.Instance.Log($"Health: {playerHealth}");
    }

    [Command("SetHealth", Description = "Sets player health to specified amount")]
    private void SetHealth(int amount)
    {
        playerHealth = amount;
        ConsoleManager.Instance.Log($"Health set to {playerHealth}");
    }
}
```

That's all you need! The console will automatically discover and register these commands when the scene loads.

### 4. Use Commands

Open the console and type:
```
GetHealth
SetHealth 75
```

## 📖 Usage Guide

### Creating Commands

Commands are created by adding the `[Command]` attribute to any method:

```csharp
using PPConsole;

public class ExampleCommands : MonoBehaviour
{
    // Basic command - no parameters
    [Command("SayHello", Description = "Prints a greeting")]
    private void SayHello()
    {
        ConsoleManager.Instance.Log("Hello, World!");
    }

    // Command with parameters
    [Command("Teleport", Description = "Teleports player to coordinates")]
    private void Teleport(float x, float y, float z)
    {
        transform.position = new Vector3(x, y, z);
        ConsoleManager.Instance.Log($"Teleported to ({x}, {y}, {z})");
    }

    // Command with string parameter (use quotes for spaces)
    [Command("Say", Description = "Makes character speak")]
    private void Say(string message)
    {
        ConsoleManager.Instance.Log($"Character says: '{message}'");
    }
}
```

### Command with Quoted Arguments

For string arguments containing spaces, use quotes:

```
Say "Hello World"
Teleport 10 5 20
```

### Supported Parameter Types

The console automatically converts arguments to these types:
- `string`
- `int`, `long`
- `float`, `double`
- `bool`
- `enum` types
- Any type supported by `Convert.ChangeType()`

### Build Stripping

By default, commands are **stripped from production builds** and only available in the Unity Editor or Development Builds. This protects sensitive developer commands:

```csharp
// This command will be stripped from production builds (default behavior)
[Command("GodMode", Description = "Toggles invincibility")]
private void ToggleGodMode(bool enabled)
{
    // Only available in Editor/Development builds
}

// Set strip: false to keep command in all builds
[Command("GetVersion", strip: false, Description = "Shows game version")]
private void GetVersion()
{
    ConsoleManager.Instance.Log($"Version: {Application.version}");
    // Available in all builds, including production
}
```

**Strip Parameter:**
- `strip: true` (default) - Command only available in Editor/Development builds
- `strip: false` - Command available in all builds including production

### Static Commands

Static methods can also be commands and don't require an instance:

```csharp
[Command("QuitGame", strip: false, Description = "Exits the application")]
private static void QuitGame()
{
    ConsoleManager.Instance.Log("Quitting...");
    Application.Quit();
}
```

### Manual Registration

For non-MonoBehaviour classes or runtime registration:

```csharp
public class CustomCommands
{
    [Command("CustomCommand")]
    private void MyCommand()
    {
        // Command logic
    }
}

// Register manually
var customCommands = new CustomCommands();
ConsoleManager.Instance.RegisterCommandsFromObject(customCommands);
```

## Built-in Commands

The console includes these commands out of the box:

| Command | Description |
|---------|-------------|
| `help` | Lists all available commands with their signatures |
| `clear` | Clears the console output |
| `echo <text>` | Echoes back the provided text |
| `history` | Shows command history |

## ⚙️ Customization

### Console Appearance

The console UI can be customized by editing the `ConsoleUI` prefab:
- Change colors, fonts, and sizes in the TextMeshPro components
- Modify the panel background, opacity, and layout
- Adjust the scroll view behavior

### Console Settings

Adjust settings in the `ConsoleUI` component:
- **Max Log Entries** - Maximum number of log entries to keep (default: 500)

### Logging from Your Code

Log messages to the console from anywhere in your code:

```csharp
ConsoleManager.Instance.Log("This is a normal message");
ConsoleManager.Instance.LogWarning("This is a warning");
ConsoleManager.Instance.LogError("This is an error");
```

## Architecture

### Core Components

- **`ConsoleManager`** - Singleton that manages command registration, discovery, and execution
- **`ConsoleUI`** - UI component that renders the console and handles user input
- **`CommandAttribute`** - Attribute to mark methods as console commands
- **`CommandInfo`** - Internal class that stores command metadata

### Command Discovery

The console automatically discovers commands in two ways:

1. **Static Methods** - Scans all assemblies for static methods with `[Command]` attribute
2. **Instance Methods** - Finds all MonoBehaviours in the scene and scans them for `[Command]` methods

Commands are discovered during `Start()` after all `Awake()` and `OnEnable()` calls have completed.

## 🤝 Contribute

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

### Development Setup

1. Clone the repository
2. Open the project in Unity 2021.3 or later
3. Open the `DemoScene` in `Samples/Scenes/` to see examples
4. Make your changes and test thoroughly

### Guidelines

- Follow existing code style and conventions
- Add XML documentation comments to public APIs
- Test changes in both Editor and builds
- Update README if adding new features

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Roadmap

Future features:
- [ ] Autocomplete suggestions

---

Made with ❤️ by PlayerMadeGames
