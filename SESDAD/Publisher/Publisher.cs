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

    public class Publisher : Process, IPublisher {
        private int seqNumber;

        public Publisher(ProcessHeader newProcessHeader) :
            base(newProcessHeader) {
                seqNumber = 1;
        }

        public int SegNumber {
            get { return seqNumber++; }
        }

        public override void ServiceInit() {
            base.ServiceInit();
            RemotingServices.Marshal(
                new PublisherService((IPublisher)this),
                serviceName,
                typeof(PublisherService));
        }

        public override void ConnectToPuppetMaster(String newPuppetMasterURL) {
            base.ConnectToPuppetMaster(newPuppetMasterURL);
            PuppetMaster.RegisterPublisher(ProcessHeader);
            Console.WriteLine("Connected to " + newPuppetMasterURL);
        }
        public override void ConnectToParentBroker(String newParentURL) {
            base.ConnectToParentBroker(newParentURL);
            ParentBroker.RegisterPublisher(ProcessHeader);
            Console.WriteLine("Connected to " + newParentURL);
        }

        public void Publish(String topicName, String content) {
            Entry entry = new Entry(topicName, content, ProcessHeader, seqNumber);
            ParentBroker.Publish(ProcessHeader, entry);
            Console.WriteLine("I published a new entry about <" + topicName + ">");
        }

        public void Debug() {
            String debugMessage =
                "**********************************************" + Environment.NewLine +
                "* Hello, I'm a Publisher. Here's my info:" + Environment.NewLine +
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
            ProcessHeader processHeader = new ProcessHeader(args[0], ProcessType.PUBLISHER, args[1], args[2]);
            Publisher publisher = new Publisher(processHeader);

            publisher.ServiceInit();
            publisher.ConnectToPuppetMaster(args[3]);
            publisher.ConnectToParentBroker(args[4]);
            publisher.Debug();

            Console.ReadLine();
        }
    }
}
