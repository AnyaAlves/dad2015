using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SESDAD.CommonTypes;

namespace SESDAD.Processes {
    public class MessageBrokerService : MarshalByRefObject, IBrokerService {
        private IPuppetMasterService puppetMaster;
        private IMessageBroker process;

        public MessageBrokerService(IMessageBroker newBroker)
            : base() {
            process = newBroker;
        }
        ///<summary>
        /// Broker Remote Service name
        ///</summary>
        public String ProcessName {
            get { return process.ProcessName; }
        }
        ///<summary>
        /// Broker Remote Service routing policy
        ///</summary>
        public RoutingPolicyType RoutingPolicy {
            set { process.RoutingPolicy = value; }
        }
        ///<summary>
        /// Broker Remote Service ordering
        ///</summary>
        public OrderingType Ordering {
            set { process.Ordering = value; }
        }

        public void Publish(ProcessHeader processHeader, Entry entry) {
            process.ForwardEntry(processHeader, entry);
            puppetMaster.WriteIntoFullLog("BroEvent " + process.ProcessName + ", " + processHeader.ProcessName + ", " + entry.TopicName + ", " + entry.SeqNumber);
        }

        public void Subscribe(ProcessHeader processHeader, String topicName) {
            process.registerSubscription(processHeader, topicName);
        }

        public void Unsubscribe(ProcessHeader processHeader, String topicName) {
            process.removeSubscription(processHeader, topicName);
        }

        public void RegisterBroker(ProcessHeader processHeader) {
            process.RegisterBroker(processHeader);
        }

        public void RegisterSubscriber(ProcessHeader processHeader) {
            process.RegisterSubscriber(processHeader);
        }

        public void RegisterPublisher(ProcessHeader processHeader) {
            process.RegisterPublisher(processHeader);
        }

        public void ConnectToPuppetMaster(String puppetMasterURL) {
            puppetMaster = (IPuppetMasterService)Activator.GetObject(
                 typeof(IPuppetMasterService),
                 puppetMasterURL);
            Console.WriteLine("Connected to PuppetMaster.");
        }

        public void ConnectToParentBroker(String parentbrokerURL) {
            process.ConnectToParentBroker(parentbrokerURL);
        }

        public void ForceFreeze() {
            process.Freeze();
        }
        public void ForceUnfreeze() {
            process.Unfreeze();
        }
        public void ForceCrash() {
            process.Crash();
        }

        public void TryPing() { }
    }
}
