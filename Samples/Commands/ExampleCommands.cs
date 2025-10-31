using UnityEngine;

namespace PPConsole.Examples
{
    /// <summary>
    /// Example script demonstrating how to use the [Command] attribute.
    /// Attach this to any GameObject to register these commands.
    /// </summary>
    public class ExampleCommands : MonoBehaviour
    {
        [Header("Example Settings")]
        [SerializeField] private int health = 100;
        [SerializeField] private string playerName = "Player";

        // Commands are automatically discovered by the ConsoleManager on Awake
        // No need to manually register!

        // ===== Example Commands =====

        /// <summary>
        /// Simple command with no arguments.
        /// Usage: GetHealth
        /// </summary>
        [Command("GetHealth", strip: false, Description = "Displays the current health value")]
        private void GetHealth()
        {
            ConsoleManager.Instance.Log($"Current health: {health}");
        }

        /// <summary>
        /// Command with a single integer argument.
        /// Usage: SetHealth 75
        /// </summary>
        [Command("SetHealth", strip: true, Description = "Sets the health to a specific value")]
        private void SetHealth(int amount)
        {
            health = amount;
            ConsoleManager.Instance.Log($"Health set to: {health}");
        }

        /// <summary>
        /// Command with multiple arguments, matching your example.
        /// Usage: AddHealth navi 40
        /// </summary>
        [Command("AddHealth", strip: true, Description = "Adds health to a specific player")]
        private void AddHealth(string targetPlayerName, int amount)
        {
            if (targetPlayerName.Equals(playerName, System.StringComparison.OrdinalIgnoreCase))
            {
                health += amount;
                ConsoleManager.Instance.Log($"Added {amount} health to {targetPlayerName}. New health: {health}");
            }
            else
            {
                ConsoleManager.Instance.LogWarning($"Player '{targetPlayerName}' not found. Current player is '{playerName}'");
            }
        }

        /// <summary>
        /// Command with float argument.
        /// Usage: SetTimeScale 0.5
        /// </summary>
        [Command("SetTimeScale", strip: true, Description = "Changes the game's time scale")]
        private void SetTimeScale(float scale)
        {
            Time.timeScale = scale;
            ConsoleManager.Instance.Log($"Time scale set to: {scale}");
        }

        /// <summary>
        /// Command with boolean argument.
        /// Usage: ToggleGodMode true
        /// </summary>
        [Command("ToggleGodMode", strip: true, Description = "Enables or disables god mode")]
        private void ToggleGodMode(bool enabled)
        {
            string status = enabled ? "enabled" : "disabled";
            ConsoleManager.Instance.Log($"God mode {status}");
            // Your god mode logic here
        }

        /// <summary>
        /// Command with string argument that can contain spaces (use quotes).
        /// Usage: Speak "Hello World"
        /// </summary>
        [Command("Speak", strip: false, Description = "Makes the character speak")]
        private void Speak(string message)
        {
            ConsoleManager.Instance.Log($"{playerName} says: '{message}'");
        }

        /// <summary>
        /// Command that will ALWAYS be available, even in production builds.
        /// Usage: GetVersion
        /// </summary>
        [Command("GetVersion", strip: false, Description = "Shows the game version")]
        private void GetVersion()
        {
            ConsoleManager.Instance.Log($"Game Version: {Application.version}");
        }

        /// <summary>
        /// Static command example - doesn't require an instance.
        /// Usage: QuitGame
        /// </summary>
        [Command("QuitGame", strip: false, Description = "Quits the application")]
        private static void QuitGame()
        {
            ConsoleManager.Instance.Log("Quitting application...");
            
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
