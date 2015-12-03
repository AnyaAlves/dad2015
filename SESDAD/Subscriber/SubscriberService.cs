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
            if (Process.Frozen)
            {
                Process.Freeze(new Task(() => { Process.ReceiveEvent(@event); }));
                return;
            }
            Process.ReceiveEvent(@event);
        }

        public void ForceSubscribe(String topicName) {
            Console.Write("Subscribed to: " + topicName + "\n");
            if (Process.Frozen)
            {
                Process.Freeze(new Task(() => { Process.Subscribe(topicName); }));
                return;
            }
            Process.Subscribe(topicName);
        }
        public void ForceUnsubscribe(String topicName) {
            if (Process.Frozen)
            {
                Process.Freeze(new Task(() => { Process.Unsubscribe(topicName); }));
                return;
            }
            Process.Unsubscribe(topicName);
        }
    }
}
