using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace PPConsole
{
    /// <summary>
    /// Core manager for the debug console system.
    /// Handles command registration, discovery, and execution.
    /// </summary>
    public class ConsoleManager : MonoBehaviour
    {
        private static ConsoleManager _instance;
        public static ConsoleManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[PPConsole Manager]");
                    _instance = go.AddComponent<ConsoleManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        public static bool Exists => _instance != null;

        private Dictionary<string, CommandInfo> _commands = new Dictionary<string, CommandInfo>(StringComparer.OrdinalIgnoreCase);
        private List<string> _commandHistory = new List<string>();
        private const int MaxHistorySize = 100;

        public event Action<string, LogType> OnLogMessage;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void Start()
        {
            // Register built-in commands first
            RegisterBuiltInCommands();
            
            // Then discover commands from scene after all Awake/OnEnable have run
            DiscoverCommands();
        }

        /// <summary>
        /// Manually register a command from a specific object instance.
        /// Useful for non-MonoBehaviour classes or runtime registration.
        /// </summary>
        public void RegisterCommandsFromObject(object target)
        {
            if (target == null)
            {
                LogError("Cannot register commands from null object");
                return;
            }

            var methods = target.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            
            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttribute<CommandAttribute>();
                if (attribute != null)
                {
                    RegisterCommand(attribute, method, method.IsStatic ? null : target);
                }
            }
        }

        /// <summary>
        /// Discovers all methods marked with [Command] attribute across all assemblies.
        /// </summary>
        private void DiscoverCommands()
        {
            // First, discover static commands from all types in the assembly
            DiscoverStaticCommands();

            // Then find all MonoBehaviours in the scene for instance commands
            var allMonoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            
            foreach (var mb in allMonoBehaviours)
            {
                // Skip ConsoleManager itself since we already registered built-in commands
                if (mb == this)
                    continue;
                    
                RegisterCommandsFromObject(mb);
            }

            Log($"Discovered {_commands.Count} commands");
        }

        /// <summary>
        /// Discovers all static methods marked with [Command] attribute.
        /// </summary>
        private void DiscoverStaticCommands()
        {
            // Get all assemblies
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            
            foreach (var assembly in assemblies)
            {
                // Skip system assemblies for performance
                if (assembly.FullName.StartsWith("System") || 
                    assembly.FullName.StartsWith("Unity") ||
                    assembly.FullName.StartsWith("mscorlib"))
                    continue;

                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        // Get all static methods
                        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                        
                        foreach (var method in methods)
                        {
                            var attribute = method.GetCustomAttribute<CommandAttribute>();
                            if (attribute != null)
                            {
                                RegisterCommand(attribute, method, null);
                            }
                        }
                    }
                }
                catch (System.Exception)
                {
                    // Skip assemblies that can't be loaded
                    continue;
                }
            }
        }

        /// <summary>
        /// Registers a command from a method and its attribute.
        /// </summary>
        private void RegisterCommand(CommandAttribute attribute, MethodInfo method, object target)
        {
            var commandInfo = new CommandInfo(
                attribute.CommandName,
                attribute.Description,
                method,
                target,
                attribute.Strip
            );

            // Check if command should be available in current build
            if (!commandInfo.IsAvailableInCurrentBuild())
            {
                return;
            }

            if (_commands.ContainsKey(attribute.CommandName))
            {
                // Silently overwrite - no need to warn the user
                return;
            }

            _commands[attribute.CommandName] = commandInfo;
        }

        /// <summary>
        /// Executes a command string.
        /// </summary>
        public void ExecuteCommand(string commandString)
        {
            if (string.IsNullOrWhiteSpace(commandString))
            {
                return;
            }

            // Add to history
            _commandHistory.Add(commandString);
            if (_commandHistory.Count > MaxHistorySize)
            {
                _commandHistory.RemoveAt(0);
            }

            Log($"> {commandString}");

            // Parse command and arguments
            var parts = ParseCommandString(commandString);
            if (parts.Length == 0)
            {
                return;
            }

            string commandName = parts[0];
            string[] args = parts.Skip(1).ToArray();

            // Find and execute command
            if (_commands.TryGetValue(commandName, out var commandInfo))
            {
                try
                {
                    ExecuteCommandInfo(commandInfo, args);
                }
                catch (Exception ex)
                {
                    LogError($"Error executing command '{commandName}': {ex.Message}\n{ex.StackTrace}");
                }
            }
            else
            {
                LogError($"Unknown command: '{commandName}'. Type 'help' for a list of available commands.");
            }
        }

        /// <summary>
        /// Executes a CommandInfo with the provided arguments.
        /// </summary>
        private void ExecuteCommandInfo(CommandInfo commandInfo, string[] args)
        {
            var parameters = commandInfo.Parameters;

            // Check parameter count
            if (args.Length != parameters.Length)
            {
                LogError($"Command '{commandInfo.CommandName}' expects {parameters.Length} argument(s), but {args.Length} were provided.\nUsage: {commandInfo.GetSignature()}");
                return;
            }

            // Convert arguments to proper types
            object[] convertedArgs = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                try
                {
                    convertedArgs[i] = ConvertArgument(args[i], parameters[i].ParameterType);
                }
                catch (Exception ex)
                {
                    LogError($"Failed to convert argument '{args[i]}' to type {parameters[i].ParameterType.Name}: {ex.Message}");
                    return;
                }
            }

            // Invoke the method
            commandInfo.Method.Invoke(commandInfo.Target, convertedArgs);
        }

        /// <summary>
        /// Converts a string argument to the target type.
        /// </summary>
        private object ConvertArgument(string arg, Type targetType)
        {
            if (targetType == typeof(string))
            {
                return arg;
            }

            if (targetType == typeof(int))
            {
                return int.Parse(arg);
            }

            if (targetType == typeof(float))
            {
                return float.Parse(arg);
            }

            if (targetType == typeof(bool))
            {
                return bool.Parse(arg);
            }

            if (targetType == typeof(double))
            {
                return double.Parse(arg);
            }

            if (targetType == typeof(long))
            {
                return long.Parse(arg);
            }

            if (targetType.IsEnum)
            {
                return Enum.Parse(targetType, arg, true);
            }

            // For other types, try Convert.ChangeType
            return Convert.ChangeType(arg, targetType);
        }

        /// <summary>
        /// Parses a command string into command name and arguments.
        /// Supports quoted strings for arguments with spaces.
        /// </summary>
        private string[] ParseCommandString(string commandString)
        {
            List<string> parts = new List<string>();
            bool inQuotes = false;
            string currentPart = "";

            for (int i = 0; i < commandString.Length; i++)
            {
                char c = commandString[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (char.IsWhiteSpace(c) && !inQuotes)
                {
                    if (currentPart.Length > 0)
                    {
                        parts.Add(currentPart);
                        currentPart = "";
                    }
                }
                else
                {
                    currentPart += c;
                }
            }

            if (currentPart.Length > 0)
            {
                parts.Add(currentPart);
            }

            return parts.ToArray();
        }

        /// <summary>
        /// Gets all available commands.
        /// </summary>
        internal IEnumerable<CommandInfo> GetAllCommands()
        {
            return _commands.Values;
        }

        /// <summary>
        /// Gets command history.
        /// </summary>
        public IReadOnlyList<string> GetCommandHistory()
        {
            return _commandHistory.AsReadOnly();
        }

        /// <summary>
        /// Logs a message to the console.
        /// </summary>
        public void Log(string message)
        {
            Debug.Log($"[PPConsole] {message}");
            OnLogMessage?.Invoke(message, LogType.Log);
        }

        /// <summary>
        /// Logs a warning to the console.
        /// </summary>
        public void LogWarning(string message)
        {
            Debug.LogWarning($"[PPConsole] {message}");
            OnLogMessage?.Invoke(message, LogType.Warning);
        }

        /// <summary>
        /// Logs an error to the console.
        /// </summary>
        public void LogError(string message)
        {
            Debug.LogError($"[PPConsole] {message}");
            OnLogMessage?.Invoke(message, LogType.Error);
        }

        #region Built-in Commands

        private void RegisterBuiltInCommands()
        {
            RegisterCommandsFromObject(this);
        }

        [Command("help", false, Description = "Lists all available commands")]
        private void HelpCommand()
        {
            Log("=== Available Commands ===");
            
            var sortedCommands = _commands.Values.OrderBy(c => c.CommandName);
            
            foreach (var cmd in sortedCommands)
            {
                string description = string.IsNullOrEmpty(cmd.Description) ? "" : $" - {cmd.Description}";
                Log($"  {cmd.GetSignature()}{description}");
            }
        }

        [Command("clear", false, Description = "Clears the console output")]
        private void ClearCommand()
        {
            OnLogMessage?.Invoke("", LogType.Log); // Special signal to clear
            Log("Console cleared");
        }

        [Command("echo", false, Description = "Echoes back the provided text")]
        private void EchoCommand(string text)
        {
            Log(text);
        }

        [Command("history", false, Description = "Shows command history")]
        private void HistoryCommand()
        {
            Log("=== Command History ===");
            for (int i = 0; i < _commandHistory.Count; i++)
            {
                Log($"{i + 1}: {_commandHistory[i]}");
            }
        }

        #endregion
    }
}
