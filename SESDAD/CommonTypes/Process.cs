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

namespace SESDAD.CommonTypes {

    public abstract class Process {
        protected String processName;
        protected Site site;
        protected String processURL;
        protected int portNumber;

        protected ObjRef serviceReference;
        protected TcpChannel channel;

        public Process(String processName, Site site, String processURL) {
            this.processName = processName;
            this.site = site;
            this.processURL = processURL;

            Regex regex = new Regex(@"tcp://[\w\.]+:(\d\d\d\d)/\w+");
            Match match = regex.Match(processURL);
            Int32.TryParse(match.Groups[1].Value, out portNumber);
        }

        public abstract void Connect();

        //public abstract void Disconnect();
    }
}
