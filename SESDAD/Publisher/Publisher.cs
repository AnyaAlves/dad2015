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


namespace SESDAD.Publisher {

    public class Publisher : Process {

        private String brokerURL;
        private IBrokerRemoteService brokerService;
        private IAdministratorService administratorService;

        public Publisher(String processName, String siteName, String processURL, String brokerURL)
                : base(processName, siteName, processURL) {
            this.brokerURL = brokerURL;
        }

        public override void Connect() {
            base.Connect();
            brokerService = (IBrokerRemoteService)Activator.GetObject(
                typeof(IBrokerRemoteService),
                brokerURL);

            administratorService = (IAdministratorService)Activator.GetObject(
                typeof(IAdministratorService),
                "tcp://localhost:1000/PuppetMasterService");
            administratorService.ConfirmPublisherConnection(processName, processURL);
        }

        public void Publish(String topicName, String content) {
            Entry entry = new Entry(topicName, content);
            brokerService.Publish(processName, processURL, entry);
            Console.WriteLine("I published a new entry about <" + topicName + ">");
        }

        public void Debug() {
            String debugMessage =
                "**********************************************" + "\n" +
                "* Hello, I'm a Publisher. Here's my info:" + "\n" +
                "* Process Name: " + processName + "\n" +
                "* Site Name: " + siteName + "\n" +
                "* Process URL: " + processURL + "\n" +
                "* Port Number: " + portNumber + "\n" +
                "* Parent URL: " + brokerURL + "\n" +
                "**********************************************" + "\n";
            Console.Write(debugMessage);
        }
    }

    class Program {
        static void Main(string[] args) {
            Publisher publisher = new Publisher(args[0], args[1], args[2], args[3]);
            publisher.Debug();
            publisher.Connect();
            publisher.Publish("Cenas Fixes", "Isto fala sobre cenas bueda fixes");
            Console.ReadLine();
        }
    }
}
