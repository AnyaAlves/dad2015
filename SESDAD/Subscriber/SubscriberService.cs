using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SESDAD.CommonTypes;

namespace SESDAD.Processes {
    public class SubscriberService : GenericProcessService<ISubscriber>, ISubscriberService {
        public void DeliverEntry(Entry entry) {
            PuppetMaster.WriteIntoLog(
                "SubEvent " +
                Header.ProcessName + ", " +
                entry.PublisherHeader.ProcessName + ", " +
                entry.TopicName + ", " +
                entry.SeqNumber);
            Process.ReceiveEntry(entry);
        }

        public void ForceSubscribe(String topicName) {
            Process.Subscribe(topicName);
        }
        public void ForceUnsubscribe(String topicName) {
            Process.Unsubscribe(topicName);
        }
    }
}
