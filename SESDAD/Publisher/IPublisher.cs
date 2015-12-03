using System;

using SESDAD.Processes;
using SESDAD.Commons;

namespace SESDAD.Processes {
    public interface IPublisher : IGenericProcess {
        int SeqNumber { get; }

        void Publish(String topicName, String content);
        //void Ack(int seqNumber);
    }
}
