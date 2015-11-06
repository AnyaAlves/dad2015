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

    public class Subscriber : GenericProcess, ISubscriber {
        public Subscriber(ProcessHeader newProcessHeader) :
            base(newProcessHeader) {
        }

        public void Subscribe(String topicName) {
            Monitor.Enter(WaitingObject);
            ParentBroker.Subscribe(Header, topicName);
            Monitor.Pulse(WaitingObject);
            Monitor.Exit(WaitingObject);
        }
        public void Unsubscribe(String topicName) {
            Monitor.Enter(WaitingObject);
            ParentBroker.Unsubscribe(Header, topicName);
            Monitor.Pulse(WaitingObject);
            Monitor.Exit(WaitingObject);
        }

        public void ReceiveEntry(Entry entry) {
            String nl = Environment.NewLine;

            Console.WriteLine("New event received!" + nl +
                "Topic Name: " + entry.TopicName + nl +
                "Content: " + entry.Content + nl +
                "Publisher: " + nl + entry.PublisherHeader + nl);
            Monitor.Enter(WaitingObject);
            ParentBroker.AckDelivery(Header, entry.PublisherHeader);            
            Monitor.Pulse(WaitingObject);
            Monitor.Exit(WaitingObject);
        }

        public override void ConnectToParentBroker(String parentBrokerURL) {
            base.ConnectToParentBroker(parentBrokerURL);
            Monitor.Enter(WaitingObject);
            ParentBroker.RegisterSubscriber(Header);
            Monitor.Pulse(WaitingObject);
            Monitor.Exit(WaitingObject);
        }

        public override String ToString() {
            String nl = Environment.NewLine;

            return
                "**********************************************" + nl +
                " Subscriber :" + nl +
                base.ToString() + nl +
                "**********************************************" + nl;
        }
    }

    class Program {
        static void Main(string[] args) {
            ProcessHeader processHeader = new ProcessHeader(args[0], ProcessType.SUBSCRIBER, args[1], args[2]);
            Subscriber process = new Subscriber(processHeader);

            process.LaunchService<SubscriberService, ISubscriber>(((ISubscriber)process));
            process.ConnectToParentBroker(args[3]);

            Console.WriteLine(process);
            Console.ReadLine();
        }
    }
}
