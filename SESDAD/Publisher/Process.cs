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

namespace SESDAD.Publisher {

    public abstract class Process {

        protected String processName;
        protected String siteName;
        protected String processURL;

        protected int portNumber;
        protected String serviceName;
        protected ObjRef serviceReference;
        protected TcpChannel channel;

        public Process(String processName, String siteName, String processURL) {
            this.processName = processName;
            this.siteName = siteName;
            this.processURL = processURL;

            //extracting the port number and service name
            String URLpattern = @"tcp://[\w\.]+:(\d\d\d\d)/(\w+)";
            Match match = Regex.Match(processURL, URLpattern);
            Int32.TryParse(match.Groups[1].Value, out portNumber);
            serviceName = match.Groups[2].Value;
        }

        public virtual void Connect() {
            channel = new TcpChannel(portNumber);
            ChannelServices.RegisterChannel(channel, true);
        }

        //public abstract void Disconnect();
    }
}
