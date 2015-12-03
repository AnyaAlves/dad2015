using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SESDAD.Commons {
    public interface IMessageBrokerService : IGenericProcessService {
        RoutingPolicyType RoutingPolicy { set; }
        OrderingType Ordering { set; }
        IList<ProcessHeader> ReplicatedBrokerList { get; }

        void ConnectToMainBroker(String mainbrokerURL);

        //subscriber->broker
        void RegisterSubscriber(ProcessHeader subscriberHeader);
        void Subscribe(ProcessHeader subscriberHeader, String topicName);
        void Unsubscribe(ProcessHeader subscriberHeader, String topicName);
        void AckDelivery(ProcessHeader subscriberHeader, ProcessHeader publisherHeader);

        //publisher->broker
        void Publish(Event @event);

        //child->broker
        void RegisterBroker(ProcessHeader childBrokerHeader);
        //broker->brokers
        void SpreadSubscription(ProcessHeader brokerHeader, String topicName);
        void SpreadUnsubscription(ProcessHeader brokerHeader, String topicName);
        void MulticastEvent(EventContainer eventContainer);
        void UnicastEvent(EventContainer eventContainer);
    }
}