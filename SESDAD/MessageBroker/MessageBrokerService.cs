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

        //subscriber->broker
        public void RegisterSubscriber(ProcessHeader subscriberHeader) {
            Process.AddSubscriber(subscriberHeader);
        }

        public void Subscribe(ProcessHeader subscriberHeader, String topicName) {
            Process.MakeSubscription(subscriberHeader, topicName);
        }
        public void Unsubscribe(ProcessHeader subscriberHeader, String topicName) {
            Process.RemoveSubscription(subscriberHeader, topicName);
        }

        public void AckDelivery(ProcessHeader subscriberHeader, ProcessHeader publisherHeader) {
            Process.AckDelivery(subscriberHeader, publisherHeader);
        }

        //publisher->broker
        public void Publish(Entry entry) {
            Process.SubmitEntry(entry);
        }

        //child->broker
        public void RegisterChildBroker(ProcessHeader childBrokerHeader) {
            Process.AddChildBroker(childBrokerHeader);
        }
        //broker->parent
        public void SpreadSubscription(ProcessHeader brokerHeader, String topicName) {
            Process.SpreadSubscription(brokerHeader, topicName);
        }
        //broker->brokers
        public void MulticastEntry(ProcessHeader senderBrokerHeader, Entry entry) {
            PuppetMaster.WriteIntoFullLog(
                "BroEvent " +
                Header.ProcessName + ", " +
                entry.PublisherHeader.ProcessName + ", " +
                entry.TopicName + ", " +
                entry.SeqNumber);
            Process.MulticastEntry(senderBrokerHeader, entry);
        }

    }
}
