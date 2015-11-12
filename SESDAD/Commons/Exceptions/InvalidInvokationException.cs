using System;
using System.Runtime.Serialization;

namespace SESDAD.Commons.Exceptions {
    public abstract class InvalidInvokationException : ApplicationException {
        public InvalidInvokationException(string methodName, ProcessHeader header, Exception exception)
            : base("Invalid " + methodName + " on " + header.ProcessType + " " + header.ProcessName, exception ) {
        }

        public InvalidInvokationException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
        }
    }
}
