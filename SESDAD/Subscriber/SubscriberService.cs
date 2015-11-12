using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SESDAD.Commons;

namespace SESDAD.Processes {
    public class SubscriberService : GenericProcessService<ISubscriber>, ISubscriberService {
        public void DeliverEvent(Event @event) {
            PuppetMaster.WriteIntoLog(
                "SubEvent " +
                Header.ProcessName + ", " +
                @event.PublisherHeader.ProcessName + ", " +
                @event.TopicName + ", " +
                @event.SeqNumber);
            Process.ReceiveEvent(@event);
        }

        public void ForceSubscribe(String topicName) {
            Console.Write("Subscribed to: " + topicName + "\n");
            Process.Subscribe(topicName);
        }
        public void ForceUnsubscribe(String topicName) {
            Process.Unsubscribe(topicName);
        }
    }
}
