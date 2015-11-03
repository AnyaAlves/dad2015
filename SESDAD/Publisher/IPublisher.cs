using System;

using SESDAD.Processes;
using SESDAD.CommonTypes;

namespace SESDAD.Processes {
    public interface IPublisher : IGenericProcess {

        Entry Publish(String topicName, String content);
        //void Ack(int seqNumber);
    }
}
