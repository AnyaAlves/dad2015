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

using SESDAD.Commons;


namespace SESDAD.Processes {

    public class Subscriber : GenericProcess, ISubscriber {
        public Subscriber(ProcessHeader newProcessHeader) :
            base(newProcessHeader) {
        }

        public void Subscribe(String topicName) {
            try {
                ParentBroker.Subscribe(Header, topicName);
            } catch (SocketException) {
                Crash();
            }
        }
        public void Unsubscribe(String topicName) {
            try {
                ParentBroker.Unsubscribe(Header, topicName);
            } catch (SocketException) {
                Crash();
            }
        }

        public void ReceiveEvent(Event @event) {
            Console.WriteLine("New event received!\n" + @event);
            try {
                ParentBroker.AckDelivery(Header, @event.PublisherHeader);
            } catch (SocketException) {
                Crash();
            }
        }

        public override void ConnectToParentBroker(String parentBrokerURL) {
            base.ConnectToParentBroker(parentBrokerURL);
            try {
                ParentBroker.RegisterSubscriber(Header);
            } catch (SocketException) {
                Crash();
            }
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
