using System;
using System.Runtime.Serialization;

namespace SESDAD.CommonTypes.Exceptions {
    [Serializable]
    public class InvalidConnectionException : InvalidInvokationException {
        public InvalidConnectionException(ProcessHeader header, Exception exception)
            : base("Connection", header, exception) {
        }

        public InvalidConnectionException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
        }
    }
}
