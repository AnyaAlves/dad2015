using System;

namespace SESDAD.Managing.Exceptions {
    public class InvalidCommandException : Exception {
        public InvalidCommandException(String command)
            : base ("Invalid command " + command) {
        }
    }
}
