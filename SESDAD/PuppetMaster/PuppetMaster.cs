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
using System.Threading;

using System.Diagnostics;

namespace SESDAD.PuppetMaster {
    public class PuppetMaster {
        private IDictionary<String, IPuppetMasterService> URLToPuppetMasterServiceTable;
        private IDictionary<String, IPuppetMasterService> brokerTable, publisherTable, subscriberTable;
        private RoutingPolicyType routingPolicy;
        private OrderingType ordering;
        private LoggingLevelType loggingLevel;
        private IDictionary<String, String> siteToPuppetMasterURLTable;
        private const int PORT = 1000;
        private SESDAD.CommonTypes.Site site;

        public PuppetMaster() {
            URLToPuppetMasterServiceTable = new Dictionary<String, IPuppetMasterService>();
            brokerTable = new Dictionary<String, IPuppetMasterService>();
            publisherTable = new Dictionary<String, IPuppetMasterService>();
            subscriberTable = new Dictionary<String, IPuppetMasterService>();
            routingPolicy = RoutingPolicyType.flooding;
            ordering = OrderingType.FIFO;
            loggingLevel = LoggingLevelType.light;
            siteToPuppetMasterURLTable = new Dictionary<String, String>();
            siteToPuppetMasterURLTable.Add("Site0", "localhost");
        }

        //<summary>
        // Starts connection with Puppet Master Service
        //</summary>
        public void Connect() {
            TcpChannel channel = new TcpChannel(PORT);
            ChannelServices.RegisterChannel(channel, true);
            PuppetMasterService puppetMasterService = new PuppetMasterService();
            RemotingServices.Marshal(
                puppetMasterService,
                "PuppetMasterService",
                typeof(PuppetMasterService));
        }

        public void ExecuteConfigurationFile() {
            String line, reply;
            StreamReader file = new StreamReader("sesdadrc");
            while ((line = file.ReadLine()) != null) {
                reply = parseLineToCommand(line);
                System.Console.WriteLine(reply);
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

        private bool TryGetService(String process, out IPuppetMasterService service) {
            return ((brokerTable.TryGetValue(process, out service) ||
                    publisherTable.TryGetValue(process, out service) ||
                    subscriberTable.TryGetValue(process, out service)));
        }

        //<summary>
        // Identify command
        //</summary>
        private String parseLineToCommand(String line) {
            String[] fields = line.Split(' ');
            String command = fields[0];

            if (fields.Length == 1 && command.Equals("Status")) {
                foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterService in URLToPuppetMasterServiceTable) {
                    return puppetMasterService.Value.ExecuteStatusCommand();
                }
            }
            else if (fields.Length == 2 && command.Equals("Crash")) {
                IPuppetMasterService puppetMasterService;
                if (TryGetService(fields[1], out puppetMasterService)) {
                    puppetMasterService.ExecuteCrashCommand(fields[1]);
                }
                else {
                    return "Process " + fields[1] + " not found.";
                }
            }
            else if (fields.Length == 2 && command.Equals("Freeze")) {
                IPuppetMasterService puppetMasterService;
                if (TryGetService(fields[1], out puppetMasterService)) {
                    puppetMasterService.ExecuteFreezeCommand(fields[1]);
                }
                else {
                    return "Process " + fields[1] + " not found.";
                }
            }
            else if (fields.Length == 2 && command.Equals("Unfreeze")) {
                IPuppetMasterService puppetMasterService;
                if (TryGetService(fields[1], out puppetMasterService)) {
                    puppetMasterService.ExecuteUnfreezeCommand(fields[1]);
                }
                else {
                    return "Process " + fields[1] + " not found.";
                }
            }
            else if (fields.Length == 2 && command.Equals("Wait")) {
                int integerTime;
                if (Int32.TryParse(fields[1], out integerTime)) {
                    foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterService in URLToPuppetMasterServiceTable)
                        puppetMasterService.Value.ExecuteWaitCommand(integerTime);
                }
            }
            else if (fields.Length == 2 && command.Equals("RoutingPolicy")) {
                if (fields[1] == "flooding") {
                    routingPolicy = RoutingPolicyType.flooding;
                    foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterService in URLToPuppetMasterServiceTable)
                        puppetMasterService.Value.ExecuteFloodingRoutingPolicyCommand();
                }
                else if (fields[1] == "filter") {
                    routingPolicy = RoutingPolicyType.filter;
                    foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterService in URLToPuppetMasterServiceTable)
                        puppetMasterService.Value.ExecuteFilterRoutingPolicyCommand();
                }
            }
            else if (fields.Length == 2 && command.Equals("Ordering")) {
                if (fields[1] == "NO") {
                    ordering = OrderingType.NO;
                    foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterService in URLToPuppetMasterServiceTable)
                        puppetMasterService.Value.ExecuteNoOrderingCommand();
                }
                else if (fields[1] == "FIFO") {
                    ordering = OrderingType.FIFO;
                    foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterService in URLToPuppetMasterServiceTable)
                        puppetMasterService.Value.ExecuteFIFOOrderingCommand();
                }
                else if (fields[1] == "TOTAL") {
                    ordering = OrderingType.TOTAL;
                    foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterService in URLToPuppetMasterServiceTable)
                        puppetMasterService.Value.ExecuteTotalOrderingCommand();
                }
            }
            else if (fields.Length == 2 && command.Equals("LoggingLevel")) {
                if (fields[1].Equals("full")) {
                    loggingLevel = LoggingLevelType.full;
                    foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterService in URLToPuppetMasterServiceTable)
                        puppetMasterService.Value.ExecuteFullLoggingLevelCommand();
                }
                else if (fields[1].Equals("light")) {
                    loggingLevel = LoggingLevelType.light;
                    foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterService in URLToPuppetMasterServiceTable)
                        puppetMasterService.Value.ExecuteLightLoggingLevelCommand();
                }
            }
            else if (fields.Length == 4 && command.Equals("Site") && fields[2].Equals("Parent")) {
                foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterService in URLToPuppetMasterServiceTable)
                    if (fields[3].Equals("none")) {
                        puppetMasterService.Value.ExecuteRootSiteCommand(fields[1]);
                    }
                    else {
                        puppetMasterService.Value.ExecuteSiteCommand(fields[1], fields[3]);
                    }
            }
            else if (fields.Length == 4 && command.Equals("Subscriber")) {
                IPuppetMasterService puppetMasterService;
                if (subscriberTable.TryGetValue(fields[1], out puppetMasterService)) {
                    if (fields[2].Equals("Subscribe")) {
                        puppetMasterService.ExecuteSubscribeCommand(fields[1], fields[3]);
                    }
                    else if (fields[2].Equals("Unsubscribe")) {
                        puppetMasterService.ExecuteUnsubscribeCommand(fields[1], fields[3]);
                    }
                }
                else {
                    return "Subscriber " + fields[1] + " not found.";
                }
            }
            else if (fields.Length == 8 &&
                     command.Equals("Publisher") &&
                     fields[2].Equals("Publish") &&
                     fields[4].Equals("Ontopic") &&
                     fields[6].Equals("Interval")) {
                int publishTimes, intervalTimes;
                if (Int32.TryParse(fields[3], out publishTimes) &&
                    Int32.TryParse(fields[7], out intervalTimes)) {
                    IPuppetMasterService puppetMasterService;
                    if (publisherTable.TryGetValue(fields[1], out puppetMasterService)) {
                        puppetMasterService.ExecutePublishCommand(
                                fields[1],
                                publishTimes,
                                fields[5],
                                intervalTimes);
                    }
                    else {
                        return "Subscriber " + fields[1] + " not found.";
                    }
                }
            }
            else if (fields.Length == 8 && command.Equals("Process") &&
                        fields[2].Equals("Is") &&
                        fields[4].Equals("On") &&
                        fields[6].Equals("URL") &&
                        new Regex(@"^tcp://[\w\.]+:\d{1,5}/\w+$").IsMatch(fields[7])) {
                String pattern = @"tcp://([\w\.]+):\d{1,5}/\w+", url = Regex.Match(fields[7], pattern).Groups[1].Value;
                IPuppetMasterService puppetMasterService;
                if (!siteToPuppetMasterURLTable.Contains(new KeyValuePair<String, String>(fields[5], url))) {
                    return "Site " + fields[5] + " not found.";
                }
                if (!URLToPuppetMasterServiceTable.TryGetValue(url, out puppetMasterService)) {
                    puppetMasterService = (IPuppetMasterService)Activator.GetObject(
                           typeof(IPuppetMasterService),
                           @"tcp://" + url + ":" + PORT + @"/PuppetMasterService");
                    URLToPuppetMasterServiceTable.Add(url, puppetMasterService);
                }
                if (fields[3].Equals("broker")) {
                    puppetMasterService.ExecuteBrokerCommand(
                            fields[1],
                            fields[5],
                            fields[7]);
                    brokerTable.Add(fields[1], puppetMasterService);
                }
                else if (fields[3].Equals("publisher")) {
                    puppetMasterService.ExecutePublisherCommand(
                            fields[1],
                            fields[5],
                            fields[7]);
                    publisherTable.Add(fields[1], puppetMasterService);
                }
                else if (fields[3].Equals("subscriber")) {
                    puppetMasterService.ExecuteSubscriberCommand(
                            fields[1],
                            fields[5],
                            fields[7]);
                    subscriberTable.Add(fields[1], puppetMasterService);
                }
            }
            return line;
        }
    }

    public static class Program {
        //<summary>
        // Entry Point
        //</summary>
        public static void Main(string[] args) {
            PuppetMaster puppetMaster = new PuppetMaster();
            puppetMaster.Connect();
            System.Console.WriteLine("Connected to PuppetMasterURL");
            if (args.Length == 1 && File.Exists(args[0])) {
                puppetMaster.ExecuteConfigurationFile();
                puppetMaster.StartCLI();
            }
            System.Console.ReadLine();
        }
    }
}
