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

        public void Publish(String processName, String processURL, Entry entry) {
            broker.ForwardEntry(processName, processURL, entry);
        }

        public void Subscribe(String processName, String processURL, String topicName) {
            broker.registerSubscription(processName, processURL, topicName);
        }

        public void Unsubscribe(String processName, String processURL, String topicName) {
            broker.removeSubscription(processName, processURL, topicName);
        }

        public void RegisterBroker(String processName, String processURL) {
            broker.RegisterBroker(processName, processURL);
        }

        public void RegisterSubscriber(String processName, String processURL) {
            broker.RegisterSubscriber(processName, processURL);
        }

        public void RegisterPublisher(String processName, String processURL) {
            broker.RegisterPublisher(processName, processURL);
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
