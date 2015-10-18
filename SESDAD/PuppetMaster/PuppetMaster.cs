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
        private IList<IPuppetMasterService> puppetMasters;
        private IDictionary<String, IPuppetMasterService> serviceTable;
        private IDictionary<String, IPuppetMasterService> brokenServiceTable;
        private IDictionary<String, String> brokerTable, publisherTable, subscriberTable;
        private RoutingPolicyType routingPolicy;
        private OrderingType ordering;
        private LoggingLevelType loggingLevel;
        private IDictionary<String, String> siteToPuppetMasterURLTable;

        public PuppetMaster() {
            puppetMasters = new List<IPuppetMasterService>();
            serviceTable = new Dictionary<String, IPuppetMasterService>();
            brokenServiceTable = new Dictionary<String, IPuppetMasterService>();
            brokerTable = new Dictionary<String, String>();
            publisherTable = new Dictionary<String, String>();
            subscriberTable = new Dictionary<String, String>();
            routingPolicy = RoutingPolicyType.flooding;
            ordering = OrderingType.FIFO;
            loggingLevel = LoggingLevelType.light;
            siteToPuppetMasterURLTable = new Dictionary<String, String>();
            siteToPuppetMasterURLTable.Add("Site0","tcp://localhost:1000");
            siteToPuppetMasterURLTable.Add("Site1","tcp://localhost:1001");
            siteToPuppetMasterURLTable.Add("Site2","tcp://localhost:1002");
            siteToPuppetMasterURLTable.Add("Site3","tcp://localhost:1003");
            siteToPuppetMasterURLTable.Add("Site4","tcp://localhost:1004");
            siteToPuppetMasterURLTable.Add("Site5","tcp://localhost:1005");
            siteToPuppetMasterURLTable.Add("Site6","tcp://localhost:1006");
            siteToPuppetMasterURLTable.Add("Site7","tcp://localhost:1007");
            siteToPuppetMasterURLTable.Add("Site8","tcp://localhost:1008");
            siteToPuppetMasterURLTable.Add("Site9","tcp://localhost:1009");
        }

        //<summary>
        // Starts connection with Puppet Master Service
        //</summary>
        public void Connect(String site, int port) {
            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, true);
            PuppetMasterService puppetMasterService = new PuppetMasterService();
            RemotingServices.Marshal(
                puppetMasterService,
                "tcp:" + site + ":" + port + "/PuppetMasterURL",
                typeof(PuppetMasterService));
            System.Console.WriteLine("Connected to tcp:" + site + ":" + port + "/PuppetMasterURL");
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

        private String putServiceAside(String key) {
            IPuppetMasterService brokenPuppetMasterService;
            if (serviceTable.TryGetValue(key, out brokenPuppetMasterService)) {
                brokenServiceTable.Add(key, brokenPuppetMasterService);
                serviceTable.Remove(key);
                return "Site " + key + " removed from list of Services.";
            }
            return "Site does not exist";
        }

        private bool TryGetService(String process, out String serviceName, out IPuppetMasterService service) {
            service = null;
            return ((brokerTable.TryGetValue(process, out serviceName) ||
                    publisherTable.TryGetValue(process, out serviceName) ||
                    subscriberTable.TryGetValue(process, out serviceName)) &&
                    serviceTable.TryGetValue(serviceName, out service));
        }

        //<summary>
        // Identify command
        //</summary>
        private String parseLineToCommand(String line) {
            String[] fields = line.Split(' ');
            String command = fields[0];

            if (fields.Length == 1 && command.Equals("Status")) {
                foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterService in serviceTable)
                        return puppetMasterService.Value.ExecuteStatusCommand();
                return line;
            }
            else if (fields.Length == 2 && command.Equals("Crash")) {
                String service;
                IPuppetMasterService puppetMasterService;
                if (TryGetService(fields[1], out service, out puppetMasterService)) {
                        puppetMasterService.ExecuteCrashCommand(fields[1]);
                    return line;
                }
                else {
                    return "Process " + fields[1] + " not found.";
                }
            }
            else if (fields.Length == 2 && command.Equals("Freeze")) {
                String service;
                IPuppetMasterService puppetMasterService;
                if (TryGetService(fields[1], out service, out puppetMasterService)) {
                        puppetMasterService.ExecuteFreezeCommand(fields[1]);
                    return line;
                }
                else {
                    return "Process " + fields[1] + " not found.";
                }
            }
            else if (fields.Length == 2 && command.Equals("Unfreeze")) {
                String service;
                IPuppetMasterService puppetMasterService;
                if (TryGetService(fields[1], out service, out puppetMasterService)) {
                        puppetMasterService.ExecuteUnfreezeCommand(fields[1]);
                    return line;
                }
                else {
                    return "Process " + fields[1] + " not found.";
                }
            }
            else if (fields.Length == 2 && command.Equals("Wait")) {
                int integerTime;
                if (Int32.TryParse(fields[1], out integerTime)) {
                    foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterService in serviceTable)
                            puppetMasterService.Value.ExecuteWaitCommand(integerTime);
                    return line;
                }
            }
            else if (fields.Length == 2 && command.Equals("RoutingPolicy")) {
                if (fields[1] == "flooding") {
                    routingPolicy = RoutingPolicyType.flooding;
                    foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterService in serviceTable)
                            puppetMasterService.Value.ExecuteFloodingRoutingPolicyCommand();
                    return line;
                }
                else if (fields[1] == "filter") {
                    routingPolicy = RoutingPolicyType.filter;
                    foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterService in serviceTable)
                            puppetMasterService.Value.ExecuteFilterRoutingPolicyCommand();
                    return line;
                }
            }
            else if (fields.Length == 2 && command.Equals("Ordering")) {
                if (fields[1] == "NO") {
                    ordering = OrderingType.NO;
                    foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterService in serviceTable)
                            puppetMasterService.Value.ExecuteNoOrderingCommand();
                    return line;
                }
                else if (fields[1] == "FIFO") {
                    ordering = OrderingType.FIFO;
                    foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterService in serviceTable)
                            puppetMasterService.Value.ExecuteFIFOOrderingCommand();
                    return line;
                }
                else if (fields[1] == "TOTAL") {
                    ordering = OrderingType.TOTAL;
                    foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterService in serviceTable)
                            puppetMasterService.Value.ExecuteTotalOrderingCommand();
                    return line;
                }
            }
            else if (fields.Length == 2 && command.Equals("LoggingLevel")) {
                if (fields[1].Equals("full")) {
                    loggingLevel = LoggingLevelType.full;
                    foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterService in serviceTable)
                            puppetMasterService.Value.ExecuteFullLoggingLevelCommand();
                    return line;
                }
                else if (fields[1].Equals("light")) {
                    loggingLevel = LoggingLevelType.light;
                    foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterService in serviceTable)
                            puppetMasterService.Value.ExecuteLightLoggingLevelCommand();
                    return line;
                }
            }
            else if (fields.Length == 4 && command.Equals("Site") && fields[2].Equals("Parent")) {
                // Assumption : The site has necessarily a Puppet Master
                String url;
                if (siteToPuppetMasterURLTable.TryGetValue(fields[1], out url)) {
                    // Let's pretend WE are creating the Puppet Master
                    System.Diagnostics.Process.Start("SESDAD.PuppetMaster.exe", url.Replace(":", " "));
                    Thread.Sleep(10000);
                    // I'm not going to China to make a simple execute
                    IPuppetMasterService puppetMasterService = (IPuppetMasterService)Activator.GetObject(
                        typeof(IPuppetMasterService),
                        url + "/PuppetMasterURL");
                        puppetMasterService.SetPolicies(routingPolicy, ordering, loggingLevel);
                        if (fields[3].Equals("none")) {
                            puppetMasterService.ExecuteRootSiteCommand(fields[1]);
                        }
                        else {
                            puppetMasterService.ExecuteSiteCommand(fields[1], fields[3]);
                        }
                    serviceTable.Add(fields[1], puppetMasterService);
                    return line;
                }
                else {
                    return "Site " + fields[1] + " not found.";
                }
            }
            else if (fields.Length == 4 && command.Equals("Subscriber")) {
                if (fields[2].Equals("Subscribe")) {
                    String service;
                    IPuppetMasterService puppetMasterService;
                    if (subscriberTable.TryGetValue(fields[1], out service) &&
                        serviceTable.TryGetValue(service, out puppetMasterService)) {
                            puppetMasterService.ExecuteSubscribeCommand(fields[1], fields[3]);
                        return line;
                    }
                    else {
                        return "Subscriber " + fields[1] + " not found.";
                    }
                }
                else if (fields[2].Equals("Unsubscribe")) {
                    String service;
                    IPuppetMasterService puppetMasterService;
                    if (subscriberTable.TryGetValue(fields[1], out service) &&
                        serviceTable.TryGetValue(service, out puppetMasterService)) {
                            puppetMasterService.ExecuteUnsubscribeCommand(fields[1], fields[3]);
                        return line;
                    }
                    else {
                        return "Subscriber " + fields[1] + " not found.";
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
                        return line;
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
                        new Regex("^tcp://\\w+:\\d\\d\\d\\d/\\w+$").IsMatch(fields[7])) {
                if (fields[3].Equals("broker")) {
                    IPuppetMasterService puppetMasterService;
                    if (serviceTable.TryGetValue(fields[5], out puppetMasterService)) {
                            puppetMasterService.ExecuteBrokerCommand(
                                    fields[1],
                                    fields[5],
                                    fields[7]);
                            brokerTable.Add(fields[1], fields[5]);
                        return line;
                    }
                    else {
                        return "Site " + fields[5] + " not found.";
                    }
                }
                else if (fields[3].Equals("publisher")) {
                    IPuppetMasterService puppetMasterService;
                    if (serviceTable.TryGetValue(fields[5], out puppetMasterService)) {
                            puppetMasterService.ExecutePublisherCommand(
                                    fields[1],
                                    fields[5],
                                    fields[7]);
                            publisherTable.Add(fields[1], fields[5]);
                        return line;
                    }
                    else {
                        return "Site " + fields[5] + " not found.";
                    }
                }
                else if (fields[3].Equals("subscriber")) {
                    IPuppetMasterService puppetMasterService;
                    if (serviceTable.TryGetValue(fields[5], out puppetMasterService)) {
                            puppetMasterService.ExecuteSubscriberCommand(
                                    fields[1],
                                    fields[5],
                                    fields[7]);
                            subscriberTable.Add(fields[1], fields[5]);
                        return line;
                    }
                    else {
                        return "Site " + fields[5] + " not found.";
                    }
                }
            }
            return "Invalid Command";
        }
    }

    public static class Program {
        //<summary>
        // Entry Point
        //</summary>
        public static void Main(string[] args) {
            if (args.Length <= 3) {
                PuppetMaster puppetMaster = new PuppetMaster();
                String url;
                int port;
                switch(args.Length) {
                    case 1 :
                        url = "//localhost";
                        port = 1000;
                        break;
                    case 3:
                        url = args[1];
                        port = Int32.Parse(args[2]);
                        break;
                    default:
                        return;
                }
                puppetMaster.Connect(url, port);
                if (args.Length == 1 && File.Exists(args[0])) {
                    puppetMaster.ExecuteConfigurationFile();
                    puppetMaster.StartCLI();
                }
                System.Console.ReadLine();
            }
        }
    }
}
