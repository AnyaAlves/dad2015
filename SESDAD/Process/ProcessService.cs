using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SESDAD.CommonTypes;

namespace SESDAD.Processes {
    public class ProcessService<I> : MarshalByRefObject
        where I : IProcess {
        private I process;
        private IPuppetMasterService puppetMaster;

        public I Process {
            get { return process; }
            set { process = value; }
        }
        public IPuppetMasterService PuppetMaster {
            get { return puppetMaster; }
        }
        public ProcessHeader ProcessHeader {
            get { return process.ProcessHeader; }
        }

        public void ConnectToPuppetMaster(String puppetMasterURL) {
            puppetMaster = (IPuppetMasterService)Activator.GetObject(
                 typeof(IPuppetMasterService),
                 puppetMasterURL);
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
