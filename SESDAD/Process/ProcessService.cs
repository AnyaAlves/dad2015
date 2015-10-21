using System;

using SESDAD.CommonTypes;

namespace SESDAD.Processes {
    public abstract class ProcessService : MarshalByRefObject {
        private IPuppetMasterRemoteService puppetMaster;
        private String brokerName;

        public ProcessService(String newBrokerName) :
            base() {
            brokerName = newBrokerName;
        }

        protected IPuppetMasterRemoteService PuppetMaster {
            get { return puppetMaster; }
        }

        protected String BrokerName {
            get { return brokerName; }
        }

        public void ConnectToPuppetMaster(String puppetMasterURL) {
            puppetMaster = (IPuppetMasterRemoteService)Activator.GetObject(
                 typeof(IPuppetMasterRemoteService),
                 puppetMasterURL);
        }

        public void ConnectToBroker(String brokerURL) {

        }

        public void Freeze() { }
        public void Unfreeze() { }
        public void Crash() {
            Environment.Exit(-1);
        }
    }
}
