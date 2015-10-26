using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SESDAD.CommonTypes;
using SESDAD.Processes;

namespace SESDAD.Processes {
    public interface ISubscriber : IProcess {
        void Subscribe(String topicName);
        void Unsubscribe(String topicName);

        void ReceiveEntry(Entry entry);
    }
}
