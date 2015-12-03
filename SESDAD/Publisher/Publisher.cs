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

    public class Publisher : GenericProcess, IPublisher {
        public int SeqNumber { get; private set; }

        public Publisher(ProcessHeader newProcessHeader) :
            base(newProcessHeader) {
                SeqNumber = 0;
        }

        public void Publish(String topicName, String content) {
            int seqNumber = SeqNumber++;
            Event @event = new Event(topicName, content, Header, seqNumber);
            Action<Event> method = ParentBroker.Publish;
            try {
                method.BeginInvoke(@event, DonePublishing, method);
            } catch (SocketException) {
                Crash();
            }
        }

        private void DonePublishing(IAsyncResult result) {
            var target = (Action<Event>)result.AsyncState;
            target.EndInvoke(result);
        }

        public override String ToString() {
            String nl = Environment.NewLine;

            return
                "**********************************************" + nl +
                " Publisher :" + nl +
                base.ToString() + nl +
                "**********************************************" + nl +
                " Sequence Number: " + SeqNumber + nl +
                "**********************************************" + nl;
        }
    }

    class Program {
        static void Main(string[] args) {
            ProcessHeader processHeader = new ProcessHeader(args[0], ProcessType.PUBLISHER, args[1], args[2]);
            Publisher process = new Publisher(processHeader);

            process.LaunchService<PublisherService, IPublisher>(((IPublisher)process));
            process.ConnectToParentBroker(args[3]);
            
            Console.WriteLine(process);
            Console.ReadLine();
        }
    }
}
