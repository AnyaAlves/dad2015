using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SESDAD.CommonTypes;

namespace SESDAD.Processes {
    public class SubscriberService : MarshalByRefObject, ISubscriberService {
        private IPuppetMasterService puppetMaster;
        private ISubscriber process;

        public SubscriberService(ISubscriber newSubscriber) :
            base() {
            process = newSubscriber;
        }

        public void DeliverEntry(Entry entry) {
            process.DeliverEntry(entry);
            puppetMaster.WriteIntoFullLog("SubEvent " + process.ProcessName + ", " + entry.PublisherHeader.ProcessName + ", " + entry.TopicName + ", " + entry.SeqNumber);
        }

        public void ForceSubscribe(String topicName) {
            process.Subscribe(topicName);
        }
        public void ForceUnsubscribe(String topicName) {
            process.Unsubscribe(topicName);
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
