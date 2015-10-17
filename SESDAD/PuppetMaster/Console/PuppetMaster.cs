using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using System.Net.Sockets;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Text.RegularExpressions;
using SESDAD.PuppetMaster.CommonTypes;
using System.Threading;

using System.Diagnostics;

namespace SESDAD.PuppetMaster {
    public class PuppetMaster {
        private IList<IPuppetMasterService> puppetMasters;
        private IDictionary<String, IPuppetMasterService> serviceTable;
        private IDictionary<String, String> brokerTable, publisherTable, subscriberTable;
        private int port;

        public PuppetMaster() {
            puppetMasters = new List<IPuppetMasterService>();
            serviceTable = new Dictionary<String, IPuppetMasterService>();
            brokerTable = new Dictionary<String, String>();
            publisherTable = new Dictionary<String, String>();
            subscriberTable = new Dictionary<String, String>();
            port = 1000;
        }

        //<summary>
        // Starts connection with Puppet Master Service
        //</summary>
        public void Connect() {
            TcpChannel channel = new TcpChannel(port++);
            ChannelServices.RegisterChannel(channel, true);
            PuppetMasterService puppetMasterService = new PuppetMasterService();
            RemotingServices.Marshal(
                puppetMasterService,
                "PuppetMasterURL",
                typeof(PuppetMasterService));
            serviceTable.Add("127.0.0.1", (IPuppetMasterService)puppetMasterService);
        }

        public void ExecuteConfigurationFile() {
            String line;
            StreamReader file = new StreamReader("sesdadrc");
            while ((line = file.ReadLine()) != null) {
                parseLineToCommand(line);
            }
            file.Close();
        }

        //<summary>
        // Create CLI interface for user interaction with Puppet Master Service
        //</summary>
        public void StartCLI() {
            String command, reply;
            while (true) {
                command = System.Console.ReadLine();
                reply = parseLineToCommand(command);
                System.Console.WriteLine(reply);
            }
        }

        //<summary>
        // Identify command
        //</summary>
        private String parseLineToCommand(String line) {
            String[] fields = line.Split(' ');
            String command = fields[0];

            if (fields.Length == 1 && command.Equals("Status")) {
                foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterService in serviceTable)
                    puppetMasterService.Value.ExecuteStatusCommand();
            }
            else if (fields.Length == 2 && command.Equals("Crash")) {
                String service;
                IPuppetMasterService puppetMasterService;
                if ((brokerTable.TryGetValue(fields[1], out service) ||
                    publisherTable.TryGetValue(fields[1], out service) ||
                    subscriberTable.TryGetValue(fields[1], out service)) &&
                    serviceTable.TryGetValue(service, out puppetMasterService)) {
                    puppetMasterService.ExecuteCrashCommand(fields[1]);
                }
                else {
                    System.Console.WriteLine("Process " + fields[1] + " not found.");
                }
            }
            else if (fields.Length == 2 && command.Equals("Freeze")) {
                String service;
                IPuppetMasterService puppetMasterService;
                if ((brokerTable.TryGetValue(fields[1], out service) ||
                    publisherTable.TryGetValue(fields[1], out service) ||
                    subscriberTable.TryGetValue(fields[1], out service)) &&
                    serviceTable.TryGetValue(service, out puppetMasterService)) {
                    puppetMasterService.ExecuteFreezeCommand(fields[1]);
                }
                else {
                    System.Console.WriteLine("Process " + fields[1] + " not found.");
                }
            }
            else if (fields.Length == 2 && command.Equals("Unfreeze")) {
                String service;
                IPuppetMasterService puppetMasterService;
                if ((brokerTable.TryGetValue(fields[1], out service) ||
                    publisherTable.TryGetValue(fields[1], out service) ||
                    subscriberTable.TryGetValue(fields[1], out service)) &&
                    serviceTable.TryGetValue(service, out puppetMasterService)) {
                    puppetMasterService.ExecuteUnfreezeCommand(fields[1]);
                }
                else {
                    System.Console.WriteLine("Process " + fields[1] + " not found.");
                }
            }
            else if (fields.Length == 2 && command.Equals("Wait")) {
                int integerTime;
                if (Int32.TryParse(fields[1], out integerTime)) {
                    foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterService in serviceTable)
                        puppetMasterService.Value.ExecuteWaitCommand(integerTime);
                }
            }
            else if (fields.Length == 2 && command.Equals("RoutingPolicy")) {
                if (fields[1] == "flooding") {
                    foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterService in serviceTable)
                        puppetMasterService.Value.ExecuteFloodingRoutingPolicyCommand();
                }
                else if (fields[1] == "filter") {
                    foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterService in serviceTable)
                        puppetMasterService.Value.ExecuteFilterRoutingPolicyCommand();
                }
            }
            else if (fields.Length == 2 && command.Equals("Ordering")) {
                if (fields[1] == "NO") {
                    foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterService in serviceTable)
                        puppetMasterService.Value.ExecuteNoOrderingCommand();
                }
                else if (fields[1] == "FIFO") {
                    foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterService in serviceTable)
                        puppetMasterService.Value.ExecuteFIFOOrderingCommand();
                }
                else if (fields[1] == "TOTAL") {
                    foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterService in serviceTable)
                        puppetMasterService.Value.ExecuteTotalOrderingCommand();
                }
            }
            else if (fields.Length == 2 && command.Equals("LoggingLevel")) {
                if (fields[1].Equals("full")) {
                    foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterService in serviceTable)
                        puppetMasterService.Value.ExecuteFullLoggingLevelCommand();
                }
                else if (fields[1].Equals("light")) {
                    foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterService in serviceTable)
                        puppetMasterService.Value.ExecuteLightLoggingLevelCommand();
                }
            }
            else if (fields.Length == 4 && command.Equals("Site") && fields[2].Equals("Parent")) {
                // Temporary: Main Puppet Master add it's own service into each list
                CreatePuppetMasterService(fields[1], port++);
                serviceTable.Add(fields[1], (IPuppetMasterService)Activator.GetObject(
                    typeof(IPuppetMasterService),
                    "tcp://localhost:1000/PuppetMasterURL"));
                // 
                foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterService in serviceTable)
                    puppetMasterService.Value.ExecuteSiteCommand(fields[1], fields[3]);
            }
            else if (fields.Length == 4 && command.Equals("Subscriber")) {
                if (fields[2].Equals("Subscribe")) {
                    String service;
                    IPuppetMasterService puppetMasterService;
                    if (subscriberTable.TryGetValue(fields[1], out service) &&
                        serviceTable.TryGetValue(service, out puppetMasterService)) {
                        puppetMasterService.ExecuteSubscribeCommand(fields[1], fields[3]);
                    }
                    else {
                        System.Console.WriteLine("Subscriber " + fields[1] + " not found.");
                    }
                }
                else if (fields[2].Equals("Unsubscribe")) {
                    String service;
                    IPuppetMasterService puppetMasterService;
                    if (subscriberTable.TryGetValue(fields[1], out service) &&
                        serviceTable.TryGetValue(service, out puppetMasterService)) {
                        puppetMasterService.ExecuteUnsubscribeCommand(fields[1], fields[3]);
                    }
                    else {
                        System.Console.WriteLine("Subscriber " + fields[1] + " not found.");
                    }
                }
            }
            else if (fields.Length == 8 && command.Equals("Publisher") &&
                        fields[2].Equals("Publish") &&
                        fields[4].Equals("Ontopic") &&
                        fields[6].Equals("Interval")) {
                int publishTimes, intervalTimes;
                if (Int32.TryParse(fields[3], out publishTimes) && Int32.TryParse(fields[7], out intervalTimes)) {
                    String service;
                    IPuppetMasterService puppetMasterService;
                    if (publisherTable.TryGetValue(fields[1], out service) &&
                        serviceTable.TryGetValue(service, out puppetMasterService)) {
                        puppetMasterService.ExecutePublishCommand(
                                fields[1],
                                publishTimes,
                                fields[5],
                                intervalTimes);
                    }
                    else {
                        System.Console.WriteLine("Subscriber " + fields[1] + " not found.");
                    }
                }
            }
            else if (fields.Length == 8 && command.Equals("Process") &&
                        fields[2].Equals("Is") &&
                        fields[4].Equals("On") &&
                        fields[6].Equals("URL") &&
                        new Regex("^tcp://\\w+:\\d\\d\\d\\d/\\w+$").IsMatch(fields[7])) {
                if (fields[3].Equals("broker")) {
                    IPuppetMasterService puppetMasterService;
                    if (serviceTable.TryGetValue(fields[5], out puppetMasterService)) {
                        brokerTable.Add(fields[1], fields[5]);
                        puppetMasterService.ExecuteBrokerCommand(
                                fields[1],
                                fields[5],
                                fields[7]);
                    }
                    else {
                        System.Console.WriteLine("Site " + fields[1] + " not found.");
                    }
                }
                else if (fields[3].Equals("publisher")) {
                    IPuppetMasterService puppetMasterService;
                    if (serviceTable.TryGetValue(fields[5], out puppetMasterService)) {
                        publisherTable.Add(fields[1], fields[5]);
                        puppetMasterService.ExecutePublisherCommand(
                                fields[1],
                                fields[5],
                                fields[7]);
                    }
                    else {
                        System.Console.WriteLine("Site " + fields[1] + " not found.");
                    }
                }
                else if (fields[3].Equals("subscriber")) {
                    IPuppetMasterService puppetMasterService;
                    if (serviceTable.TryGetValue(fields[5], out puppetMasterService)) {
                        subscriberTable.Add(fields[1], fields[5]);
                        puppetMasterService.ExecuteSubscriberCommand(
                                fields[1],
                                fields[5],
                                fields[7]);
                    }
                    else {
                        System.Console.WriteLine("Site " + fields[1] + " not found.");
                    }
                }
            }
            return "test";
        }

        //wtf is this
        private void CreatePuppetMasterService(String site, int port) {
            String puppetMasterURL = "tcp://" + site + ":" + (port++).ToString() + "/PuppetMasterURL";
            /*    String[] args = new[] { puppetMasterURL }; //what
                Action<String[]> thread = new Action<String[]>(SESDAD.PuppetMaster.Program.Main); //?????
                thread.BeginInvoke(args, null, null);
                serviceTable.Add(site, (IPuppetMasterService)Activator.GetObject(
                    typeof(IPuppetMasterService),
                    puppetMasterURL));
           
                System.Console.WriteLine("Ok... Now what?");
                System.Console.ReadLine();*/
        }
    }

    public static class Program {
        //<summary>
        // Entry Point
        //</summary>
        public static void Main(string[] args) {
            PuppetMaster puppetMaster = new PuppetMaster();
            puppetMaster.Connect();
            puppetMaster.ExecuteConfigurationFile();
            puppetMaster.StartCLI();
        }
    }
}
