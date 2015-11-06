using System;
using System.Runtime.Serialization;

namespace SESDAD.CommonTypes.Exceptions {
    [Serializable]
    public class InvalidSubscriptionException : InvalidInvokationException {
        public InvalidSubscriptionException(ProcessHeader header, Exception exception)
            : base("Subscription", header, exception) {
        }

        public InvalidSubscriptionException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
        }
    }
}