using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SESDAD.CommonTypes;
using SESDAD.Processes;

namespace SESDAD.Processes {
    public interface IMessageBroker : IProcess {
        ///<summary>
        /// Broker Interface name
        ///</summary>
        String ProcessName { get; }
        ///<summary>
        /// Broker Interface routing policy
        ///</summary>
        RoutingPolicyType RoutingPolicy { set; }
        ///<summary>
        /// Broker Interface ordering
        ///</summary>
        OrderingType Ordering { set; }
        void ForwardEntry(ProcessHeader processHeader, Entry entry);
        void registerSubscription(ProcessHeader processHeader, String topicName);
        void removeSubscription(ProcessHeader processHeader, String topicName);

        void RegisterBroker(ProcessHeader processHeader);
        void RegisterSubscriber(ProcessHeader processHeader);
        void RegisterPublisher(ProcessHeader processHeader);
    }
}
