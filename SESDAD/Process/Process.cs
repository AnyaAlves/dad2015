using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Net.Sockets;
using System.Threading;

using SESDAD.CommonTypes;

namespace SESDAD.Processes {

    public abstract class Process {

        private ProcessHeader processHeader;
        protected String parentBrokerName,
                         parentBrokerURL,
                         puppetMasterURL;

        protected int portNumber;
        protected String serviceName;
        protected ObjRef serviceReference;
        protected TcpChannel channel;

        private IPuppetMasterRemoteService puppetMaster;
        private IBrokerRemoteService parentBroker;

        public Process(ProcessHeader newProcessHeader) {
            processHeader = newProcessHeader;

            //extracting the port number and service name
            String URLpattern = @"^tcp://[\w\.]+:(\d{1,5})/(\w+)$";
            Match match = Regex.Match(processHeader.ProcessURL, URLpattern);
            Int32.TryParse(match.Groups[1].Value, out portNumber);
            serviceName = match.Groups[2].Value;
        }

        public ProcessHeader ProcessHeader {
            get { return processHeader; }
        }

        public String ProcessName {
            get { return processHeader.ProcessName; }
        }
        public ProcessType ProcessType {
            get { return processHeader.ProcessType; }
        }

        public String SiteName {
            get { return processHeader.SiteName; }
        }

        public String ProcessURL {
            get { return processHeader.ProcessURL; }
        }

        public String ParentBrokerName {
            get { return parentBrokerName; }
        }

        public String ParentBrokerURL {
            get { return parentBrokerURL; }
        }

        protected IPuppetMasterRemoteService PuppetMaster {
            get { return puppetMaster; }
        }

        protected IBrokerRemoteService ParentBroker {
            get { return parentBroker; }
        }

        public virtual void ServiceInit() {
            channel = new TcpChannel(portNumber);
            ChannelServices.RegisterChannel(channel, true);
        }
        public virtual void ConnectToPuppetMaster(String newPuppetMasterURL) {
            puppetMaster = (IPuppetMasterRemoteService)Activator.GetObject(
                 typeof(IPuppetMasterRemoteService),
                 newPuppetMasterURL);
            puppetMasterURL = newPuppetMasterURL;
        }
        public virtual void ConnectToParentBroker(String newParentBrokerURL) {
            parentBroker = (IBrokerRemoteService)Activator.GetObject(
                typeof(IBrokerRemoteService),
                newParentBrokerURL);
            parentBrokerName = parentBroker.ProcessName;
            parentBrokerURL = newParentBrokerURL;
        }

        public void Freeze() { }
        public void Unfreeze() { }
        public void Crash() {
            Environment.Exit(-1);
        }

        //public abstract void Disconnect();
    }
}
