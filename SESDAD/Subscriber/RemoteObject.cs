using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SESDAD.CommonTypes;

namespace SESDAD.Processes {

    public class SubscriberService : MarshalByRefObject, ISubscriberRemoteService {
        IPuppetMasterRemoteService puppetMaster;
        IBrokerRemoteService brokerService;

        public SubscriberService() : base() {}

        public void ConnectToPuppetMaster(String puppetMasterURL) {
            puppetMaster = (IPuppetMasterRemoteService)Activator.GetObject(
                 typeof(IPuppetMasterRemoteService),
                 puppetMasterURL);
        }

        public void ConnectToBroker(String brokerURL) {
            brokerService = (IBrokerRemoteService)Activator.GetObject(
                typeof(IBrokerRemoteService),
                brokerURL);
        }

        public void Freeze() { }
        public void Unfreeze() { }
        public void Crash() {
            Environment.Exit(-1);
        }

        public void Subscribe(String topicName) { }
        public void Unsubscribe(String topicName) { }

        public void DeliverEntry(Entry entry) {
            Console.WriteLine("New entry!");
            Console.WriteLine(entry.TopicName + ": " + entry.Content);
            //subscriber.receive
            //puppetMaster.WriteIntoLog
        }
    }
}
