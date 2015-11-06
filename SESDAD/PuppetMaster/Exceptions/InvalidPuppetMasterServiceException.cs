using System;

namespace SESDAD.Managing.Exceptions {
    class InvalidPuppetMasterServiceException : Exception {
        public InvalidPuppetMasterServiceException(String puppetMasterURL)
            : base ("Invalid Puppet Master at " + puppetMasterURL) {
        }
    }
}
