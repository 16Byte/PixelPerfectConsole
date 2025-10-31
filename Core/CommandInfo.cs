using System;
using System.Reflection;

namespace PPConsole
{
    /// <summary>
    /// Stores information about a registered console command.
    /// </summary>
    internal class CommandInfo
    {
        public string CommandName { get; }
        public string Description { get; }
        public MethodInfo Method { get; }
        public object Target { get; }
        public ParameterInfo[] Parameters { get; }
        public bool Strip { get; }

        public CommandInfo(string commandName, string description, MethodInfo method, object target, bool strip)
        {
            CommandName = commandName;
            Description = description;
            Method = method;
            Target = target;
            Parameters = method.GetParameters();
            Strip = strip;
        }

        /// <summary>
        /// Gets a formatted string showing the command signature.
        /// </summary>
        public string GetSignature()
        {
            if (Parameters.Length == 0)
            {
                return CommandName;
            }

            var paramStrings = new string[Parameters.Length];
            for (int i = 0; i < Parameters.Length; i++)
            {
                var param = Parameters[i];
                paramStrings[i] = $"{param.ParameterType.Name} {param.Name}";
            }

            return $"{CommandName} <{string.Join("> <", paramStrings)}>";
        }

        /// <summary>
        /// Checks if this command should be available in the current build configuration.
        /// </summary>
        public bool IsAvailableInCurrentBuild()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            return true;
#else
            return !Strip;
#endif
        }
    }
}
