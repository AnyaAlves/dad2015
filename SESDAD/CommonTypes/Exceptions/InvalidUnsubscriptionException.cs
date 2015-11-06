using System;
using System.Runtime.Serialization;

namespace SESDAD.CommonTypes.Exceptions {
    [Serializable]
    public class InvalidUnsubscriptionException : InvalidInvokationException {
        public InvalidUnsubscriptionException(ProcessHeader header, Exception exception)
            : base("Unsubscription", header, exception) {
        }

        public InvalidUnsubscriptionException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
        }
    }
}