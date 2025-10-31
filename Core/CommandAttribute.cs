using System;

namespace PPConsole
{
    /// <summary>
    /// Attribute to mark a method as a console command.
    /// Place this above any method to make it accessible via the debug console.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class CommandAttribute : Attribute
    {
        /// <summary>
        /// The name of the command as it will appear in the console.
        /// </summary>
        public string CommandName { get; }

        /// <summary>
        /// If true, this command will be stripped from builds (only available in editor/development builds).
        /// If false, the command will be available in all builds.
        /// </summary>
        public bool Strip { get; }

        /// <summary>
        /// Optional description of what the command does.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Creates a console command that will be available in all builds.
        /// </summary>
        /// <param name="commandName">The name to use when calling this command from the console.</param>
        public CommandAttribute(string commandName) : this(commandName, true)
        {
        }

        /// <summary>
        /// Creates a console command with specified build stripping behavior.
        /// </summary>
        /// <param name="commandName">The name to use when calling this command from the console.</param>
        /// <param name="strip">If true, command is only available in editor/development builds. If false, available in all builds.</param>
        public CommandAttribute(string commandName, bool strip)
        {
            CommandName = commandName;
            Strip = strip;
            Description = string.Empty;
        }
    }
}
