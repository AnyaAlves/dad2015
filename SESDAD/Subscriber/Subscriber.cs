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
        public Subscriber(ProcessHeader newProcessHeader) :
            base(newProcessHeader) {
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
            PuppetMaster.RegisterSubscriber(ProcessHeader);
            Console.WriteLine("Connected to " + newPuppetMasterURL);
        }
        public override void ConnectToParentBroker(String newParentURL) {
            base.ConnectToParentBroker(newParentURL);
            ParentBroker.RegisterSubscriber(ProcessHeader);
            Console.WriteLine("Connected to " + newParentURL);
        }

        public void Subscribe(String topicName) {
            ParentBroker.Subscribe(ProcessHeader, topicName);
            Console.WriteLine("Subscribed to <" + topicName + ">");
        }

        public void Unsubscribe(String topicName) {
            ParentBroker.Unsubscribe(ProcessHeader, topicName);
            Console.WriteLine("Unsubscribed to <" + topicName + ">");
        }

        public void DeliverEntry(Entry entry) { }

        public void Debug() {
            String debugMessage =
                "**********************************************" + Environment.NewLine +
                "* Hello, I'm a Subscriber. Here's my info:" + Environment.NewLine +
                "*" + Environment.NewLine +
                "* Site Name:    " + SiteName + Environment.NewLine +
                "* Process Name: " + ProcessName + Environment.NewLine +
                "* Process URL:  " + ProcessURL + Environment.NewLine +
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
            ProcessHeader processHeader = new ProcessHeader(args[0], ProcessType.SUBSCRIBER, args[1], args[2]);
            Subscriber subscriber = new Subscriber(processHeader);

            subscriber.ServiceInit();
            subscriber.ConnectToPuppetMaster(args[3]);
            subscriber.ConnectToParentBroker(args[4]);
            subscriber.Debug();

            Console.ReadLine();
        }
    }
}
