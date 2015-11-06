using System;

namespace SESDAD.Managing.Exceptions {
    public class InvalidProcessServiceException: Exception {
        public InvalidProcessServiceException(String processName)
            : base ("Invalid process " + processName) {
        }
    }
}
