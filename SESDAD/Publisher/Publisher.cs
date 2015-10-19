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

namespace SESDAD.Publisher {

    public class Publisher {
        private String processName;
        private String siteName;
        private String processURL;
        private String brokerURL;
        private int portNumber;
        //service attributes
        private ObjRef serviceReference;
        private TcpChannel channel;
        private IBrokerRemoteService brokerService;

        public Publisher(String processName, String siteName, String processURL, String brokerURL) {
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
        }

        public void Publish(String topicName, String content) {
            new Entry(topicName, content);
            brokerService.Publish(processName, processURL, topicName);
        }
    }

    class Program {
        static void Main(string[] args) {
            Publisher publisher0 = new Publisher("publisher0", "site0", "tcp://localhost:8082/pub", "tcp://localhost:8080/broker");
            publisher0.Connect();
            publisher0.Publish("Cenas Fixes", "jdgskuayhcbsiufcglasijk");
            Console.WriteLine("Hello I'm a Publisher");
            Console.ReadLine();
        }
    }
}
