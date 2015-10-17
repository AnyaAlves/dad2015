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

    public class Publisher : Process {
        private IBrokerService brokerService;

        public Publisher(String processName, Site site, String processURL) :
            base (processName, site, processURL) {
        }

        public override void Connect() {
            channel = new TcpChannel(portNumber);
            ChannelServices.RegisterChannel(channel, true);

            brokerService = (IBrokerService)Activator.GetObject(
                typeof(IBrokerService),
                site.BrokerURL);
        }

        public void Publish(String topicName, String content) {
            new Entry(topicName, content);
            brokerService.Publish(processName, processURL, topicName);
        }
    }

    class Program {
        static void Main(string[] args) {
            Publisher publisher0 = new Publisher("publisher0", TestApp.Program.site0, "tcp://1.2.3.4:3335/pub");
            publisher0.Connect();
            publisher0.Publish("Cenas Fixes", "jdgskuayhcbsiufcglasijk");
            Console.ReadLine();
        }
    }
}
