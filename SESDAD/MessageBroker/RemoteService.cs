using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SESDAD.CommonTypes;

namespace SESDAD.MessageBroker {
    public class BrokerRemoteService : MarshalByRefObject, IBrokerRemoteService {
        MessageBroker broker;
        public BrokerRemoteService(MessageBroker broker)
            : base() {
            this.broker = broker;
        }

//----------------------------------------Proposal---------------------------------------//
        IAdministratorService puppetMaster;
        // States
        private RoutingPolicyType routingPolicy;
        private OrderingType ordering;

        ///<summary>
        /// Broker Remote Service routing policy
        ///</summary>
        public RoutingPolicyType RoutingPolicy {
            set {
                routingPolicy = value;
            }
        }
        ///<summary>
        /// Broker Remote Service ordering
        ///</summary>
        public OrderingType Ordering {
            set {
                ordering = value;
            }
        }

        public void ConnectToPuppetMaster(String puppetMasterURL) {
            puppetMaster = (IAdministratorService)Activator.GetObject(
                 typeof(IAdministratorService),
                 puppetMasterURL);
        }

        public void Freeze() { }
        public void Unfreeze() { }
        public void Crash() {
            Environment.Exit(-1);
        }
//---------------------------------------------------------------------------------------//

        public void Publish(String processName, String processURL, Entry entry) {
            broker.ForwardEntry(processName, processURL, entry);
            //puppetMaster.WriteIntoFullLog("batata");
        }

        public void Subscribe(String processName, String processURL, String topicName) {
            broker.registerSubscription(processName, processURL, topicName);
            //puppetMaster.WriteIntoFullLog("batata");
        }

        public void Unsubscribe(String processName, String processURL, String topicName) {
            broker.removeSubscription(processName, processURL, topicName);
            //puppetMaster.WriteIntoFullLog("batata");
        }

        public void RegisterBroker(String procesName, String processURL) {
            //broker.AddChild(String processName, String processURL);
        }
    }
}
