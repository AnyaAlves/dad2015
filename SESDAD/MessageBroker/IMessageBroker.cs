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
        void AddSubscriber(ProcessHeader processHeader);
        void AddSubscription(ProcessHeader processHeader, String topicName);
        void RemoveSubscription(ProcessHeader processHeader, String topicName);
        //void Ack(ProcessHeader processHeader);

        void ForwardEntry(ProcessHeader processHeader, Entry entry);

        void AddChildBroker(ProcessHeader processHeader);
    }
}
