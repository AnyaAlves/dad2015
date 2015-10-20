using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SESDAD.CommonTypes;

namespace SESDAD.MessageBroker {
    public class BrokerRemoteService : MarshalByRefObject, IBrokerRemoteService {
        MessageBroker broker;
        IAdministratorService puppetMaster;

        public BrokerRemoteService(MessageBroker broker) : base() {
            this.broker = broker;
        }

        public void ConnectToPuppetMaster(String puppetMasterURL) {
            puppetMaster = (IAdministratorService)Activator.GetObject(
                 typeof(IAdministratorService),
                 puppetMasterURL);
        }

        public void Publish(String processName, String processURL, Entry entry) {
            broker.ForwardEntry(processName, processURL, entry);
            //puppetMaster.WriteIntoLog
        }

        public void Subscribe(String processName, String processURL, String topicName) {
            broker.registerSubscription(processName, processURL, topicName);
            //puppetMaster.WriteIntoLog
        }

        public void Unsubscribe(String processName, String processURL, String topicName) {
            broker.removeSubscription(processName, processURL, topicName);
            //puppetMaster.WriteIntoLog
        }

        public void RegisterBroker(String procesName, String processURL) {
            //broker.AddChild(String processName, String processURL);
        }
    }
}
