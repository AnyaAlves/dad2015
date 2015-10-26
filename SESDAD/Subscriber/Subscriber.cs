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

        public void Subscribe(String topicName) {
            ParentBroker.Subscribe(ProcessHeader, topicName);
        }
        public void Unsubscribe(String topicName) {
            ParentBroker.Unsubscribe(ProcessHeader, topicName);
        }

        public void ReceiveEntry(Entry entry) { }
    }

    class Program {
        static void Main(string[] args) {
            ProcessHeader processHeader = new ProcessHeader(args[0], ProcessType.SUBSCRIBER, args[1], args[2]);
            Subscriber process = new Subscriber(processHeader);

            process.LaunchService<SubscriberService, ISubscriber>(((ISubscriber)process));
            process.ConnectToParentBroker(args[3]);
            process.Debug();

            Console.ReadLine();
        }
    }
}
