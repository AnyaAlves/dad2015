using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

using BinaryTreeStruct;

namespace PuppetMaster {
    class PuppetMaster {

        IList<String> subscriberList;
        IList<String> publisherList;
        IList<String> brokerList;
        BinaryTree<String> SESDADBinaryTree;
        IDictionary<String, BinaryTreeNode<String>> SESDADTable;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void parseLine(String line) {
            String[] fields = line.Split(' ');
            String command = fields[0];

            if (command.Equals("Site") && fields.Length == 4 && fields[2].Equals("Parent")) {
                if (fields[3].Equals("none")) {
                    SESDADBinaryTree.Root = new BinaryTreeNode<String>(fields[1]);
                    SESDADTable.Add(fields[1], SESDADBinaryTree.Root);
                }
                else {
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
            }
            else if (command.Equals("Process")) {
            }
            else if (command.Equals("RoutingPolicy")) {
            }
            else if (command.Equals("Ordering")) {
            }
            else if (command.Equals("Subscriber")) {
            }
            else if (command.Equals("Publisher")) {
            }
            else if (command.Equals("Status")) {
            }
            else if (command.Equals("Crash")) {
            }
            else if (command.Equals("Freeze")) {
            }
            else if (command.Equals("Unfreeze")) {
            }
            else if (command.Equals("Wait")) {
            }
            else if (command.Equals("LoggingLevel")) {
            }
        }

        public PuppetMaster(String confFileName) {
            subscriberList = new List<String>();
            publisherList = new List<String>();
            brokerList = new List<String>();
            SESDADBinaryTree = new BinaryTree<String>();
            String line;
            StreamReader file = new StreamReader(confFileName);
            while ((line = file.ReadLine()) != null) {
                parseLine(line);
            }
            file.Close();
        }

        public void Start() {

        }
    }
}
