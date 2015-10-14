using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;

using BinaryTreeStruct;
using PuppetMasterCommonTypes;

namespace PuppetMasterService {
    //<Summary>
    // "Provides a singe console from where it is possible to control experiments"
    //</Summary>
    partial class PuppetMaster {
        private IList<String> subscriberList;
        private IList<String> publisherList;
        private IList<String> brokerList;
        private BinaryTree<String> SESDADBinaryTree;
        private IDictionary<String, BinaryTreeNode<String>> SESDADTable;

        private const string SITE_ROOT = "^Site\\ \\w+\\ Parent\\ none+$",
             SITE = "^Site\\ \\w+\\ Parent\\ \\w+$",
             BROKER = "^Process\\ \\w+\\ Is\\ broker\\ On\\ \\w+\\ URL\\ tcp:\\/{2}[\\d\\.]+\\:\\d+\\/\\w*$",
             PUBLISHER = "^Process\\ \\w+\\ Is\\ publisher\\ On\\ \\w+\\ URL\\ tcp:\\/{2}[\\d\\.]+\\:\\d+\\/\\w*$",
             SUBSCRIBER = "^Process\\ \\w+\\ Is\\ subscriber\\ On\\ \\w+\\ URL\\ tcp:\\/{2}[\\d\\.]+\\:\\d+\\/\\w*$",
             ROUTINGPOLICY_DEFAULT = "^RoutingPolicy$",
             ROUTINGPOLICY_FLOODING = "^RoutingPolicy\\ flooding$",
             ROUTINGPOLICY_FILTER = "^RoutingPolicy\\ filter$",
             ORDERING_DEFAULT = "^Ordering$",
             ORDERING_NO = "^Ordering\\ NO$",
             ORDERING_FIFO = "^Ordering\\ FIFO$",
             ORDERING_TOTAL = "^Ordering\\ TOTAL$",
             SUBSCRIBE = "^Subscriber\\ \\w+\\ Subscribe\\ \\w+?$",
             UNSUBSCRIBE = "^Subscriber\\ \\w+\\ Unsubscribe\\ \\w+?$",
             PUBLISH = "^Publisher\\ \\w+\\ Publish\\ \\d+\\ Ontopic\\ \\w+\\ Interval\\ \\d$",
             STATUS = "^Status$",
             CRASH = "^Crash\\ \\w+$",
             FREEZE = "^Freeze\\ \\w+$",
             UNFREEZE = "^Unfreeze\\ \\w+$",
             WAIT = "^Wait\\ \\d+$",
             LOGGINGLEVEL_DEFAULT = "^LoggingLevel$",
             LOGGINGLEVEL_FULL = "^LoggingLevel\\ full$",
             LOGGINGLEVEL_LIGHT = "^LoggingLevel\\ light$";

        //<Summary>
        // Identify command
        //</Summary>
        public void parseLineToCommand(String line) {
            String[] fields = line.Split(' ');
            String command = fields[0];

            if (new Regex(SITE_ROOT).IsMatch(line)) {
                System.Console.WriteLine("SITE_ROOT");
                SESDADBinaryTree.Root = new BinaryTreeNode<String>(fields[1]);
                SESDADTable.Add(fields[1], SESDADBinaryTree.Root);
            }
            else if (new Regex(SITE).IsMatch(line)) {
                System.Console.WriteLine("SITE");
                BinaryTreeNode<String> node;
                SESDADTable.TryGetValue(fields[3], out node);
                if (node != null) {
                    if (node.Left == null) {
                        node.Left = new BinaryTreeNode<String>(fields[1]);
                    }
                    else {
                        node.Right = new BinaryTreeNode<String>(fields[1]);
                    }
                }
            }
            else if (new Regex(BROKER).IsMatch(line)) {
                System.Console.WriteLine("BROKER");
            }
            else if (new Regex(PUBLISHER).IsMatch(line)) {
                System.Console.WriteLine("PUBLISHER");
            }
            else if (new Regex(SUBSCRIBER).IsMatch(line)) {
                System.Console.WriteLine("SUBSCRIBER");
            }
            else if (new Regex(ROUTINGPOLICY_DEFAULT).IsMatch(line)) {
                System.Console.WriteLine("ROUTINGPOLICY_DEFAULT");
            }
            else if (new Regex(ROUTINGPOLICY_FLOODING).IsMatch(line)) {
                System.Console.WriteLine("ROUTINGPOLICY_FLOODING");
            }
            else if (new Regex(ROUTINGPOLICY_FILTER).IsMatch(line)) {
                System.Console.WriteLine("ROUTINGPOLICY_FILTER");
            }
            else if (new Regex(ORDERING_DEFAULT).IsMatch(line)) {
                System.Console.WriteLine("ORDERING_DEFAULT");
            }
            else if (new Regex(ORDERING_FIFO).IsMatch(line)) {
                System.Console.WriteLine("ORDERING_FIFO");
            }
            else if (new Regex(ORDERING_TOTAL).IsMatch(line)) {
                System.Console.WriteLine("ORDERING_TOTAL");
            }
            else if (new Regex(SUBSCRIBE).IsMatch(line)) {
                System.Console.WriteLine("SUBSCRIBE");
            }
            else if (new Regex(UNSUBSCRIBE).IsMatch(line)) {
                System.Console.WriteLine("UNSUBSCRIBE");
            }
            else if (new Regex(PUBLISH).IsMatch(line)) {
                System.Console.WriteLine("PUBLISH");
            }
            else if (new Regex(STATUS).IsMatch(line)) {
                System.Console.WriteLine("STATUS");
            }
            else if (new Regex(CRASH).IsMatch(line)) {
                System.Console.WriteLine("CRASH");
            }
            else if (new Regex(FREEZE).IsMatch(line)) {
                System.Console.WriteLine("FREEZE");
            }
            else if (new Regex(UNFREEZE).IsMatch(line)) {
                System.Console.WriteLine("UNFREEZE");
            }
            else if (new Regex(WAIT).IsMatch(line)) {
                System.Console.WriteLine("WAIT");
            }
            else if (new Regex(LOGGINGLEVEL_DEFAULT).IsMatch(line)) {
                System.Console.WriteLine("LOGGINGLEVEL_DEFAULT");
            }
            else if (new Regex(LOGGINGLEVEL_FULL).IsMatch(line)) {
                System.Console.WriteLine("LOGGINGLEVEL_FULL");
            }
            else if (new Regex(LOGGINGLEVEL_LIGHT).IsMatch(line)) {
                System.Console.WriteLine("LOGGINGLEVEL_LIGHT");
            }
        }

        public PuppetMaster() {
            //subscriberList = new List<String>();
            //publisherList = new List<String>();
            //brokerList = new List<String>();
            //SESDADBinaryTree = new BinaryTree<String>();
            //String line;
            //StreamReader file = new StreamReader("sesdadrc");
            //while ((line = file.ReadLine()) != null) {
            //    parseLineToCommand(line);
            //}
            //file.Close();
        }

        public void Connect(int port = 8086) {
            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, true);
            PuppetMasterRemoteObject remoteObject = new PuppetMasterRemoteObject();

            RemotingServices.Marshal(
                remoteObject,
                "PuppetMasterURL",
                typeof(PuppetMasterRemoteObject));
        }
    }
}
