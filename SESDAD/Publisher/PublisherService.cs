using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SESDAD.CommonTypes;

namespace SESDAD.Processes {
    public class PublisherService : MarshalByRefObject, IPublisherService {
        private IPuppetMasterService puppetMaster;
        private IPublisher process;

        public PublisherService(IPublisher newPublisher) :
            base() {
            process = newPublisher;
        }

        public void ForcePublish(String topicName, String content) {
            Entry entry = process.Publish(topicName, content);
            PuppetMaster.WriteIntoLog("PubEvent " + process.ProcessName + ", " + process.ProcessName + ", " + entry.TopicName + ", " + entry.SeqNumber);
        }

        public IPuppetMasterService PuppetMaster {
            get { return puppetMaster; }
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
