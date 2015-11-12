using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SESDAD.Commons;

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
        public void Publish(Event @event) {
            Process.SubmitEvent(@event);
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
        public void MulticastEvent(EventContainer eventContainer) {
            PuppetMaster.WriteIntoFullLog(
                "BroEvent " +
                Header.ProcessName + ", " +
                eventContainer.Event.PublisherHeader.ProcessName + ", " +
                eventContainer.Event.TopicName + ", " +
                eventContainer.Event.SeqNumber);
            Process.MulticastEvent(eventContainer);
        }

    }
}
