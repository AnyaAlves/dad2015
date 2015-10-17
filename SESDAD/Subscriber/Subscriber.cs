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

using TestApp;

namespace SESDAD.Subscriber {

    public class Subscriber : Process {
        private IBrokerService brokerService;

        public Subscriber(String processName, Site site, String processURL) :
            base(processName, site, processURL) {
        }

        public override void Connect() {
            channel = new TcpChannel(portNumber);
            ChannelServices.RegisterChannel(channel, true);

            brokerService = (IBrokerService)Activator.GetObject(
                typeof(IBrokerService),
                site.BrokerURL);

            SubscriberService service = new SubscriberService();
            serviceReference = RemotingServices.Marshal(
                service,
                "sub",
                typeof(SubscriberService));
        }

        public void Subscribe(String topicName) {
            brokerService.Subscribe(processName, processURL, topicName);
        }

        public void Unsubscribe(String topicName) {
            brokerService.Unsubscribe(processName, processURL, topicName);
        }
    }

    class Program {
        static void Main(string[] args) {
            Subscriber subscriber0 = new Subscriber("subscriber0", TestApp.Program.site0, "tcp://1.2.3.4:3334/sub");
            subscriber0.Connect();
            subscriber0.Subscribe("Cenas Fixes");
            Console.WriteLine("Hello I'm a Subscriber");
            Console.ReadLine();
        }
    }
}
