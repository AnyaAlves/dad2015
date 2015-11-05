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
            ParentBroker.Subscribe(Header, topicName);
        }
        public void Unsubscribe(String topicName) {
            ParentBroker.Unsubscribe(Header, topicName);
        }

        public void ReceiveEntry(Entry entry) {
            String nl = Environment.NewLine;

            Console.WriteLine("New event received!" + nl +
                "Topic Name: " + entry.TopicName + nl +
                "Content: " + entry.Content + nl +
                "Publisher: " + nl + entry.PublisherHeader + nl);
            ParentBroker.AckDelivery(Header, entry.PublisherHeader);
        }

        public override void ConnectToParentBroker(String parentBrokerURL) {
            base.ConnectToParentBroker(parentBrokerURL);
            ParentBroker.RegisterSubscriber(Header);
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
