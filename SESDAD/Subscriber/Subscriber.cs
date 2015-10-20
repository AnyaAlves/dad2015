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


namespace SESDAD.Subscriber {

    public class Subscriber : Process {

        private String brokerURL;
        private IBrokerRemoteService brokerService;
        private IAdministratorService administratorService;

        public Subscriber(String processName, String siteName, String processURL, String brokerURL)
                : base(processName, siteName, processURL) {
            this.brokerURL = brokerURL;
        }

        public override void Connect() {
            base.Connect();
            brokerService = (IBrokerRemoteService)Activator.GetObject(
                typeof(IBrokerRemoteService),
                brokerURL);

            RemotingServices.Marshal(
                new SubscriberRemoteObject(),
                serviceName,
                typeof(SubscriberRemoteObject));

            administratorService = (IAdministratorService)Activator.GetObject(
                typeof(IAdministratorService),
                "tcp://localhost:1000/PuppetMasterService");
            administratorService.ConfirmConnection(processName);
        }

        public void Subscribe(String topicName) {
            brokerService.Subscribe(processName, processURL, topicName);
            Console.WriteLine("Subscribed to <" + topicName + ">");
        }

        public void Unsubscribe(String topicName) {
            brokerService.Unsubscribe(processName, processURL, topicName);
            Console.WriteLine("Unsubscribed to <" + topicName + ">");
        }

        public void Debug() {
            String debugMessage =
                "**********************************************" + "\n" +
                "* Hello, I'm a Subscriber. Here's my info:" + "\n" +
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
            Subscriber subscriber = new Subscriber(args[0], args[1], args[2], args[3]);
            subscriber.Connect();
            subscriber.Debug();
            subscriber.Subscribe("Cenas Fixes");
            Console.ReadLine();
        }
    }
}
