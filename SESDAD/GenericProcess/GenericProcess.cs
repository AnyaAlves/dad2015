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

    public abstract class GenericProcess : IGenericProcess {

        private ProcessHeader processHeader;
        private IMessageBrokerService parentBroker;

        public GenericProcess(ProcessHeader newProcessHeader) {
            processHeader = newProcessHeader;
        }

        public ProcessHeader ProcessHeader {
            get { return processHeader; }
        }
        protected IMessageBrokerService ParentBroker {
            get { return parentBroker; }
        }

        public void TcpConnect(int portNumber) {
            TcpChannel channel = new TcpChannel(portNumber);
            ChannelServices.RegisterChannel(channel, true);
        }
        public void LaunchService<Service, Interface>(Interface iProcess)
            where Service : GenericProcessService<Interface>, new()
            where Interface : IGenericProcess {

            Service service = new Service();
            service.Process = iProcess;
            
            Match match = Regex.Match(ProcessHeader.ProcessURL, @"^tcp://[\w\.]+:(\d{4,5})/(\w+)$");
            int portNumber;
            String serviceName = match.Groups[2].Value;
            portNumber = Int32.Parse(match.Groups[1].Value);

            TcpConnect(portNumber);

            RemotingServices.Marshal(
                service,
                serviceName,
                typeof(Service));
        }

        public virtual void ConnectToParentBroker(String parentBrokerURL) {
            parentBroker = (IMessageBrokerService)Activator.GetObject(
                typeof(IMessageBrokerService),
                parentBrokerURL);
        }

        public void Freeze() { }
        public void Unfreeze() { }
        public void Crash() {
            Environment.Exit(-1);
        }

        public String Status {
            get {
                return
                    "**********************************************" + Environment.NewLine +
                    "* Hello, I'm a " + processHeader.ProcessType.ToString() + ". Here's my info:" + Environment.NewLine +
                    "*" + Environment.NewLine +
                    "* Site Name:    " + processHeader.SiteName + Environment.NewLine +
                    "* Process Name: " + processHeader.ProcessName + Environment.NewLine +
                    "* Process URL:  " + processHeader.ProcessURL + Environment.NewLine +
                    "**********************************************" + Environment.NewLine;
            }
        }
    }
}
