using System;

namespace SESDAD.Managing.Exceptions {
    public class InvalidSiteException : Exception {
        public InvalidSiteException(String siteName)
            : base ("Invalid site " + siteName) {
        }
    }
}
