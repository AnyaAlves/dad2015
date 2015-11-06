using System;
using System.Runtime.Serialization;

namespace SESDAD.CommonTypes.Exceptions {
    [Serializable]
    public class InvalidPublicationException : InvalidInvokationException {
        public InvalidPublicationException(ProcessHeader header, Exception exception)
            : base("Publication", header, exception) {
        }

        public InvalidPublicationException(SerializationInfo info, StreamingContext context)
            : base(info, context) {
        }
    }
}