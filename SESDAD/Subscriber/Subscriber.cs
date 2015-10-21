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


namespace SESDAD.Processes {

    public class Subscriber : Process {

        private String brokerURL;
        private ISubscriberRemoteService subscriberService;
        private IBrokerRemoteService brokerService;

        public Subscriber(
            String processName,
            String siteName,
            String processURL) :
            base(processName, siteName, processURL) {
        }

        public override void Connect() {
            base.Connect();
            SubscriberService subscriberService = new SubscriberService();
            RemotingServices.Marshal(
                subscriberService,
                serviceName,
                typeof(SubscriberService));
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
            Subscriber subscriber;
            subscriber = new Subscriber(args[0], args[1], args[2]);
            subscriber.Connect();
            subscriber.Debug();
            subscriber.Subscribe("Cenas Fixes");
            Console.ReadLine();
        }
    }
}
