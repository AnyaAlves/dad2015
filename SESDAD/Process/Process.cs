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
        private String parentURL;

        protected int portNumber;
        protected String serviceName;
        protected ObjRef serviceReference;
        protected TcpChannel channel;

        private IBrokerService parentBroker;

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

        public String ParentURL {
            get { return parentURL; }
        }

        protected IBrokerService ParentBroker {
            get { return parentBroker; }
        }

        public void TcpConnect() {
            channel = new TcpChannel(portNumber);
            ChannelServices.RegisterChannel(channel, true);
        }

        public abstract void LaunchService();

        public void ConnectToParentBroker(String parentBrokerURL) {
            parentBroker = (IBrokerService)Activator.GetObject(
                typeof(IBrokerService),
                parentBrokerURL);
            parentURL = parentBrokerURL;
            Console.WriteLine("Connected to " + parentURL);
        }

        public void Freeze() { }
        public void Unfreeze() { }
        public void Crash() {
            Environment.Exit(-1);
        }

        public void Debug() {
            String debugMessage =
                "**********************************************" + Environment.NewLine +
                "* Hello, I'm a " + ProcessType.ToString() + ". Here's my info:" + Environment.NewLine +
                "*" + Environment.NewLine +
                "* Site Name:    " + SiteName + Environment.NewLine +
                "* Process Name: " + ProcessName + Environment.NewLine +
                "* Process URL:  " + ProcessURL + Environment.NewLine +
                "*" + Environment.NewLine +
                "* Service Name: " + serviceName + Environment.NewLine +
                "* Service Port: " + portNumber + Environment.NewLine +
                "*" + Environment.NewLine +
                "* Parent URL:   " + ParentURL + Environment.NewLine +
                "**********************************************" + Environment.NewLine;
            Console.Write(debugMessage);
        }
    }
}
