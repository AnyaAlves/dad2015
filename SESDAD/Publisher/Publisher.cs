using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Net.Sockets;
using System.Threading;

using SESDAD.CommonTypes;

namespace SESDAD.Publisher {

    public class Publisher : Process {
        private IBrokerPubService brokerService;

        public Publisher(String processName, Site site, String processURL) :
            base (processName, site, processURL) {
        }

        public override void Connect() {
            channel = new TcpChannel(portNumber);
            ChannelServices.RegisterChannel(channel, true);

            brokerService = (IBrokerPubService)Activator.GetObject(
                typeof(IBrokerPubService),
                site.BrokerURL);
        }

        public void Publish(String topicName, String content) {
            new Entry(topicName, content);
            brokerService.Publish(processName, processURL, topicName);
        }
    
    }
}
