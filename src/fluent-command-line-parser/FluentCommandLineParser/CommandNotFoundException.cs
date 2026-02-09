using System;

namespace Fclp
{
    /// <summary>
    /// Represents an error that has occurred because an expected command was not found in the parser.
    /// </summary>
    public class CommandNotFoundException : Exception
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="CommandNotFoundException"/> class.
        /// </summary>
        public CommandNotFoundException() { }

        /// <summary>
        /// Initialises a new instance of the <see cref="CommandNotFoundException"/> class.
        /// </summary>
        /// <param name="commandName"></param>
        public CommandNotFoundException(string commandName) : base("Expected command " + commandName + " was not found in the parser.") { }
    }
}