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
        public IList<ProcessHeader> ReplicatedBrokerList {
            get { return Process.ReplicatedBrokerList; }
        }

        public void ConnectToMainBroker(String mainbrokerURL) {
            Process.ConnectToMainBroker(mainbrokerURL);
        }

        //subscriber->broker
        public void RegisterSubscriber(ProcessHeader subscriberHeader) {
            if (Process.Frozen) {
                Process.Freeze(new Task(() => { Process.AddSubscriber(subscriberHeader); }));
                return;
            }
            Process.AddSubscriber(subscriberHeader);
        }

        public void Subscribe(ProcessHeader subscriberHeader, String topicName) {
            if (Process.Frozen) {
                Process.Freeze(new Task(() => { Process.MakeSubscription(subscriberHeader, topicName); }));
                return;
            }
            Process.MakeSubscription(subscriberHeader, topicName);
        }
        public void Unsubscribe(ProcessHeader subscriberHeader, String topicName) {
            if (Process.Frozen) {
                Process.Freeze(new Task(() => { Process.RemoveSubscription(subscriberHeader, topicName); }));
                return;
            }
            Process.RemoveSubscription(subscriberHeader, topicName);
        }

        public void AckDelivery(ProcessHeader subscriberHeader, ProcessHeader publisherHeader) {
            if (Process.Frozen) {
                Process.Freeze(new Task(() => { Process.AckDelivery(subscriberHeader, publisherHeader); }));
                return;
            }
            Process.AckDelivery(subscriberHeader, publisherHeader);
        }

        //publisher->broker
        public void Publish(Event @event) {
            if (Process.Frozen) {
                Process.Freeze(new Task(() => { Process.SubmitEvent(@event); }));
                return;
            }
            Process.SubmitEvent(@event);
        }

        //child->broker
        public void RegisterBroker(ProcessHeader brokerHeader) {
            if (Process.Frozen) {
                Process.Freeze(new Task(() => { Process.AddBroker(brokerHeader); }));
                return;
            }
            Process.AddBroker(brokerHeader);
        }
        //broker->brokers
        public void SpreadSubscription(ProcessHeader brokerHeader, String topicName) {
            if (Process.Frozen) {
                Process.Freeze(new Task(() => { Process.SpreadSubscription(brokerHeader, topicName); }));
                return;
            }
            Process.SpreadSubscription(brokerHeader, topicName);
        }
        public void SpreadUnsubscription(ProcessHeader brokerHeader, String topicName) {
            if (Process.Frozen) {
                Process.Freeze(new Task(() => { Process.SpreadUnsubscription(brokerHeader, topicName); }));
                return;
            }
            Process.SpreadUnsubscription(brokerHeader, topicName);
        }
        public void MulticastEvent(EventContainer eventContainer) {
            PuppetMaster.WriteIntoFullLog(
                "BroEvent " +
                Header.ProcessName + ", " +
                eventContainer.Event.PublisherHeader.ProcessName + ", " +
                eventContainer.Event.TopicName + ", " +
                eventContainer.Event.SeqNumber);
            if (Process.Frozen) {
                Process.Freeze(new Task(() => { Process.MulticastEvent(eventContainer); }));
                return;
            }
            Process.MulticastEvent(eventContainer);
        }
        public void UnicastEvent(EventContainer eventContainer) {
            PuppetMaster.WriteIntoFullLog(
                "BroEvent " +
                Header.ProcessName + ", " +
                eventContainer.Event.PublisherHeader.ProcessName + ", " +
                eventContainer.Event.TopicName + ", " +
                eventContainer.Event.SeqNumber);
            if (Process.Frozen) {
                Process.Freeze(new Task(() => { Process.UnicastEvent(eventContainer); }));
                return;
            }
            Process.UnicastEvent(eventContainer);
        }
    }
}
