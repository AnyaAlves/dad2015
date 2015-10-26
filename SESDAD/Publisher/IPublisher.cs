using System;

using SESDAD.Processes;
using SESDAD.CommonTypes;

namespace SESDAD.Processes {
    public interface IPublisher : IProcess {
        Entry Publish(String topicName, String content);
    }
}
