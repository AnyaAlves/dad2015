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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void parseLine(String line) {
            String[] fields = line.Split(' ');
            String command = fields[0];

            if (command.Equals("Site")) {
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

        public PuppetMaster (String confFileName) {
            subscriberList = new List<String>();
            publisherList = new List<String>();
            brokerList = new List<String>();
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
