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

    public class Publisher : GenericProcess, IPublisher {
        private int seqNumber;

        public Publisher(ProcessHeader newProcessHeader) :
            base(newProcessHeader) {
                seqNumber = 0;
        }

        public int SeqNumber {
            get { return seqNumber; }
        }

        public Entry Publish(String topicName, String content) {
            Entry entry = new Entry(topicName, content, ProcessHeader, seqNumber++);
            ParentBroker.Publish(ProcessHeader, entry);
            return entry;
        }
        //public void Ack(int seqNumber) {}
    }

    class Program {
        static void Main(string[] args) {
            ProcessHeader processHeader = new ProcessHeader(args[0], ProcessType.PUBLISHER, args[1], args[2]);
            Publisher process = new Publisher(processHeader);

            process.LaunchService<PublisherService, IPublisher>(((IPublisher)process));
            process.ConnectToParentBroker(args[3]);

            Console.ReadLine();
        }
    }
}
