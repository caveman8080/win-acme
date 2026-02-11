using System;

namespace Fclp
{
    /// <summary>
    /// Represents an error that has occurred because a matching Command already exists in the parser.
    /// </summary>
    public class CommandAlreadyExistsException : Exception
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="CommandAlreadyExistsException"/> class.
        /// </summary>
        public CommandAlreadyExistsException() { }

        /// <summary>
        /// Initialises a new instance of the <see cref="CommandAlreadyExistsException"/> class.
        /// </summary>
        /// <param name="commandName"></param>
        public CommandAlreadyExistsException(string commandName) : base(commandName) { }
    }
}