using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SESDAD.CommonTypes;
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
        void SubmitEntry(Entry entry);

        //child->broker
        void AddChildBroker(ProcessHeader childBrokerHeader);
        //broker->parent
        void SpreadSubscription(ProcessHeader brokerHeader, String topicName);
        //broker->brokers
        void MulticastEntry(ProcessHeader senderBrokerHeader, Entry entry, int brokerSeqNumber);

        //broker->subscribers
        //void ForwardEntries();
    }
}
