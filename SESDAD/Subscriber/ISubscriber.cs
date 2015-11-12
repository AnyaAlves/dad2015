using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SESDAD.Commons;
using SESDAD.Processes;

namespace SESDAD.Processes {
    public interface ISubscriber : IGenericProcess {
        void Subscribe(String topicName);
        void Unsubscribe(String topicName);

        void ReceiveEvent(Event @event);
    }
}
