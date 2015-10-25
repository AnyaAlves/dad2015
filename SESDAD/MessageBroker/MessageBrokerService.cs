using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SESDAD.CommonTypes;

namespace SESDAD.Processes {
    public class BrokerRemoteService : MarshalByRefObject, IBrokerRemoteService {
        IMessageBroker broker;

        public BrokerRemoteService(IMessageBroker newBroker)
            : base() {
            broker = newBroker;
        }
        ///<summary>
        /// Broker Remote Service name
        ///</summary>
        public String ProcessName {
            get { return broker.ProcessName; }
        }
        ///<summary>
        /// Broker Remote Service routing policy
        ///</summary>
        public RoutingPolicyType RoutingPolicy {
            set { broker.RoutingPolicy = value; }
        }
        ///<summary>
        /// Broker Remote Service ordering
        ///</summary>
        public OrderingType Ordering {
            set { broker.Ordering = value; }
        }

        public void Publish(ProcessHeader processHeader, Entry entry) {
            broker.ForwardEntry(processHeader, entry);
        }

        public void Subscribe(ProcessHeader processHeader, String topicName) {
            broker.registerSubscription(processHeader, topicName);
        }

        public void Unsubscribe(ProcessHeader processHeader, String topicName) {
            broker.removeSubscription(processHeader, topicName);
        }

        public void RegisterBroker(ProcessHeader processHeader) {
            broker.RegisterBroker(processHeader);
        }

        public void RegisterSubscriber(ProcessHeader processHeader) {
            broker.RegisterSubscriber(processHeader);
        }

        public void RegisterPublisher(ProcessHeader processHeader) {
            broker.RegisterPublisher(processHeader);
        }

        public void Freeze() {
            broker.Freeze();
        }
        public void Unfreeze() {
            broker.Unfreeze();
        }
        public void Crash() {
            broker.Crash();
        }

        public void Ping() { }
    }
}
