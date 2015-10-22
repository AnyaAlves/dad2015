using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SESDAD.CommonTypes;

namespace SESDAD.Processes {
    public interface ISubscriber {
        void Subscribe(String topicName);
        void Unsubscribe(String topicName);

        void DeliverEntry(Entry entry);

        void Freeze();
        void Unfreeze();
        void Crash();
    }
}
