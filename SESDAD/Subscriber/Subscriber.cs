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

    public class Subscriber {
        private String processName;
        private String siteName;
        private String processURL;
        private String brokerURL;
        private int portNumber;
        //service attributes
        private ObjRef serviceReference;
        private TcpChannel channel;
        private IBrokerRemoteService brokerService;

        public Subscriber(String processName, String siteName, String processURL, String brokerURL) {
            this.processName = processName;
            this.siteName = siteName;
            this.processURL = processURL;
            this.brokerURL = brokerURL;
        }

        public void Connect() {
            channel = new TcpChannel(portNumber);
            ChannelServices.RegisterChannel(channel, true);

            brokerService = (IBrokerRemoteService)Activator.GetObject(
                typeof(IBrokerRemoteService),
                brokerURL);

            SubscriberRemoteObject service = new SubscriberRemoteObject();
            RemotingServices.Marshal(
                service,
                "sub",
                typeof(SubscriberRemoteObject));
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
            Subscriber subscriber0 = new Subscriber("subscriber0", "site0", "tcp://localhost:8081/sub", "tcp://localhost:8080/broker");
            subscriber0.Connect();
            subscriber0.Subscribe("Cenas Fixes");
            Console.WriteLine("Hello I'm a Subscriber");
            Console.ReadLine();
        }
    }
}
