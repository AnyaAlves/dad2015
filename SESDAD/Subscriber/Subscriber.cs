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

    public class Subscriber : Process, ISubscriber {
        public Subscriber(
            String processName,
            String siteName,
            String processURL) :
            base(processName, siteName, processURL) {
        }

        public override void ServiceInit() {
            base.ServiceInit();
            RemotingServices.Marshal(
                new SubscriberService((ISubscriber)this),
                serviceName,
                typeof(SubscriberService));
        }

        public override void ConnectToPuppetMaster(String newPuppetMasterURL) {
            base.ConnectToPuppetMaster(newPuppetMasterURL);
            PuppetMaster.RegisterSubscriber(processName, processURL);
            Console.WriteLine("Connected to " + newPuppetMasterURL);
        }
        public override void ConnectToParentBroker(String newParentURL) {
            base.ConnectToParentBroker(newParentURL);
            ParentBroker.RegisterSubscriber(processName, processURL);
            Console.WriteLine("Connected to " + newParentURL);
        }

        public void Subscribe(String topicName) {
            ParentBroker.Subscribe(processName, processURL, topicName);
            Console.WriteLine("Subscribed to <" + topicName + ">");
        }

        public void Unsubscribe(String topicName) {
            ParentBroker.Unsubscribe(processName, processURL, topicName);
            Console.WriteLine("Unsubscribed to <" + topicName + ">");
        }

        public void DeliverEntry(Entry entry) { }

        public void Debug() {
            String debugMessage =
                "**********************************************" + Environment.NewLine +
                "* Hello, I'm a Subscriber. Here's my info:" + Environment.NewLine +
                "*" + Environment.NewLine +
                "* Site Name:    " + siteName + Environment.NewLine +
                "* Process Name: " + processName + Environment.NewLine +
                "* Process URL:  " + processURL + Environment.NewLine +
                "*" + Environment.NewLine +
                "* Service Name: " + serviceName + Environment.NewLine +
                "* Service Port: " + portNumber + Environment.NewLine +
                "*" + Environment.NewLine +
                "* Parent Name:  " + ParentBrokerName + Environment.NewLine +
                "* Parent URL:   " + ParentBrokerURL + Environment.NewLine +
                "**********************************************" + Environment.NewLine;
            Console.Write(debugMessage);
        }
    }

    class Program {
        static void Main(string[] args) {
            Subscriber subscriber = new Subscriber(args[0], args[1], args[2]);

            subscriber.ServiceInit();
            subscriber.ConnectToPuppetMaster(args[3]);
            subscriber.ConnectToParentBroker(args[4]);
            subscriber.Debug();
            //subscriber.Subscribe("Cenas Fixes");

            Console.ReadLine();
        }
    }
}
