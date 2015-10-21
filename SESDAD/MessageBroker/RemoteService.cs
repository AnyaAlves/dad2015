using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SESDAD.CommonTypes;

namespace SESDAD.Processes {
    public class BrokerRemoteService : ProcessService, IBrokerRemoteService {
        MessageBroker broker;
        public BrokerRemoteService(MessageBroker broker, String brokerName)
            : base(brokerName) {
            this.broker = broker;
        }

        // States
        private RoutingPolicyType routingPolicy;
        private OrderingType ordering;

        ///<summary>
        /// Broker Remote Service routing policy
        ///</summary>
        public RoutingPolicyType RoutingPolicy {
            set { routingPolicy = value; }
        }
        ///<summary>
        /// Broker Remote Service ordering
        ///</summary>
        public OrderingType Ordering {
            set { ordering = value; }
        }

        public void ConnectToPuppetMaster(String puppetMasterURL) {
            base.ConnectToPuppetMaster(puppetMasterURL);
            PuppetMaster.ConfirmBrokerConnection(" ", " ");
        }

        public void Publish(String processName, String processURL, Entry entry) {
            //broker.ForwardEntry(processName, processURL, entry);
            PuppetMaster.WriteIntoFullLog("BroEvent " + BrokerName + ", " + processName + ", " + entry.TopicName + ", event-number");
        }

        public void Subscribe(String processName, String processURL, String topicName) {
            //broker.registerSubscription(processName, processURL, topicName);
        }

        public void Unsubscribe(String processName, String processURL, String topicName) {
            //broker.removeSubscription(processName, processURL, topicName);
        }

        public void RegisterBroker(String processName, String processURL) {
            //broker.AddChild(processName, processURL);
        }
    }
}
