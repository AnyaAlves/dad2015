using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SESDAD.CommonTypes;

namespace SESDAD.Processes {
    public class MessageBrokerService : GenericProcessService<IMessageBroker>, IMessageBrokerService {
        public RoutingPolicyType RoutingPolicy {
            set { Process.RoutingPolicy = value; }
        }
        public OrderingType Ordering {
            set { Process.Ordering = value; }
        }

        public void Publish(ProcessHeader processHeader, Entry entry) {
            Process.ForwardEntry(processHeader, entry);
            PuppetMaster.WriteIntoFullLog("BroEvent " + ProcessHeader.ProcessName + ", " + processHeader.ProcessName + ", " + entry.TopicName + ", " + entry.SeqNumber);
        }
        public void Subscribe(ProcessHeader processHeader, String topicName) {
            Process.AddSubscription(processHeader, topicName);
        }
        public void Unsubscribe(ProcessHeader processHeader, String topicName) {
            Process.RemoveSubscription(processHeader, topicName);
        }

        public void RegisterChildBroker(ProcessHeader processHeader) {
            Process.AddChildBroker(processHeader);
        }
    }
}
