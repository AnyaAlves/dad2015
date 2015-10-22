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
        public Publisher(
            String processName,
            String siteName,
            String processURL) :
            base(processName, siteName, processURL) {
        }

        public override void ConnectToPuppetMaster(String newPuppetMasterURL) {
            base.ConnectToPuppetMaster(newPuppetMasterURL);
            PuppetMaster.RegisterPublisher(processName, processURL);
            Console.WriteLine("Connected to " + newPuppetMasterURL);
        }
        public override void ConnectToParentBroker(String newParentURL) {
            base.ConnectToParentBroker(newParentURL);
            //ParentBroker.RegisterBroker(processURL);
            Console.WriteLine("Connected to " + newParentURL);
        }

        public void Publish(String topicName, String content) {
            Entry entry = new Entry(topicName, content);
            ParentBroker.Publish(processName, processURL, entry);
            Console.WriteLine("I published a new entry about <" + topicName + ">");
        }

        public void Debug() {
            String debugMessage =
                "**********************************************" + Environment.NewLine +
                "* Hello, I'm a Publisher. Here's my info:" + Environment.NewLine +
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
            Publisher publisher = new Publisher(args[0], args[1], args[2]);
            String[] flags = String.Join(" ", args).Split('-'),
                     parent = flags[1].Split(' ');

            publisher.Connect();
            publisher.ConnectToPuppetMaster(args[3]);
            Console.WriteLine(String.Join(" ", parent));
            if (parent.Length > 1) {
                Console.WriteLine("Ok");
                publisher.ConnectToParentBroker(parent[1]);
            }
            publisher.Debug();
            //publisher.Publish("Cenas Fixes", "Isto fala sobre cenas bueda fixes");

            Console.ReadLine();
        }
    }
}
