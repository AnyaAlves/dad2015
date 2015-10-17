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

namespace SESDAD.Subscriber {

    public class Subscriber : Process {
        private IBrokerSubService brokerService;

        public Subscriber(String processName, Site site, String processURL) :
            base(processName, site, processURL) {
        }

        public override void Connect() {
            channel = new TcpChannel(portNumber);
            ChannelServices.RegisterChannel(channel, true);

            brokerService = (IBrokerSubService)Activator.GetObject(
                typeof(IBrokerSubService),
                site.BrokerURL);

            SubscriberService service = new SubscriberService();
            serviceReference = RemotingServices.Marshal(
                service,
                "SubscriberService",
                typeof(SubscriberService));
        }

        public void Subscribe(String topicName) {
            brokerService.Subscribe(processName, processURL, topicName);
        }

        public void Unsubscribe(String topicName) {
            brokerService.Unsubscribe(processName, processURL, topicName);
        }
    }
}
