using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SESDAD.Commons;
using SESDAD.Processes;

namespace SESDAD.Processes {
    public interface IMessageBroker : IGenericProcess {
        ///<summary>
        /// Broker Interface routing policy
        ///</summary>
        RoutingPolicyType RoutingPolicy { set; }
        ///<summary>
        /// Broker Interface ordering
        ///</summary>
        OrderingType Ordering { set; }
        
        //subscriber->broker
        void AddSubscriber(ProcessHeader subscriberHeader);
        void MakeSubscription(ProcessHeader subscriberHeader, String topicName);
        void RemoveSubscription(ProcessHeader subscriberHeader, String topicName);
        void AckDelivery(ProcessHeader subscriberHeader, ProcessHeader publisherHeader);
        
        //publisher->broker
        void SubmitEvent(Event @event);

        //child->broker
        void AddBroker(ProcessHeader brokerHeader);
        //broker->brokers
        void SpreadSubscription(ProcessHeader brokerHeader, String topicName);
        void SpreadUnsubscription(ProcessHeader brokerHeader, String topicName);
        void MulticastEvent(EventContainer eventContainer);
        void UnicastEvent(EventContainer eventContainer);

        //broker->subscribers
        //void ForwardEntries();
    }
}
