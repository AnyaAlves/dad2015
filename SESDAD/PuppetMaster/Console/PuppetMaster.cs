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
        private IDictionary<String, IPuppetMasterService> brokenServiceTable;
        private IDictionary<String, String> brokerTable, publisherTable, subscriberTable;
        private const int DEFAULTPORT = 1000;

        public PuppetMaster() {
            puppetMasters = new List<IPuppetMasterService>();
            serviceTable = new Dictionary<String, IPuppetMasterService>();
            brokenServiceTable = new Dictionary<String, IPuppetMasterService>();
            brokerTable = new Dictionary<String, String>();
            publisherTable = new Dictionary<String, String>();
            subscriberTable = new Dictionary<String, String>();
        }

        //<summary>
        // Starts connection with Puppet Master Service
        //</summary>
        public void Connect() {
            TcpChannel channel = new TcpChannel(DEFAULTPORT);
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

        private void putServiceAside(String key) {
            IPuppetMasterService brokenPuppetMasterService;
            if (serviceTable.TryGetValue(key, out brokenPuppetMasterService)) {
                System.Console.WriteLine("Site " + key + " not found.");
                brokenServiceTable.Add(key, brokenPuppetMasterService);
                serviceTable.Remove(key);
                System.Console.WriteLine("Site " + key + " removed from list of Services.");
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
                    try {
                        puppetMasterService.Value.ExecuteStatusCommand();
                    }
                    catch (SocketException) {
                        putServiceAside(puppetMasterService.Key);
                    }
            }
            else if (fields.Length == 2 && command.Equals("Crash")) {
                String service;
                IPuppetMasterService puppetMasterService;
                if ((brokerTable.TryGetValue(fields[1], out service) ||
                    publisherTable.TryGetValue(fields[1], out service) ||
                    subscriberTable.TryGetValue(fields[1], out service)) &&
                    serviceTable.TryGetValue(service, out puppetMasterService)) {
                    try {
                        puppetMasterService.ExecuteCrashCommand(fields[1]);
                    }
                    catch (SocketException) {
                        putServiceAside(service);
                    }
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
                    try {
                        puppetMasterService.ExecuteFreezeCommand(fields[1]);
                    }
                    catch (SocketException) {
                        putServiceAside(service);
                    }
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
                    try {
                        puppetMasterService.ExecuteUnfreezeCommand(fields[1]);
                    }
                    catch (SocketException) {
                        putServiceAside(service);
                    }
                }
                else {
                    System.Console.WriteLine("Process " + fields[1] + " not found.");
                }
            }
            else if (fields.Length == 2 && command.Equals("Wait")) {
                int integerTime;
                if (Int32.TryParse(fields[1], out integerTime)) {
                    foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterService in serviceTable)
                        try {
                            puppetMasterService.Value.ExecuteWaitCommand(integerTime);
                        }
                        catch (SocketException) {
                            putServiceAside(puppetMasterService.Key);
                        }
                }
            }
            else if (fields.Length == 2 && command.Equals("RoutingPolicy")) {
                if (fields[1] == "flooding") {
                    foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterService in serviceTable)
                        try {
                            puppetMasterService.Value.ExecuteFloodingRoutingPolicyCommand();
                        }
                        catch (SocketException) {
                            putServiceAside(puppetMasterService.Key);
                        }
                }
                else if (fields[1] == "filter") {
                    foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterService in serviceTable)
                        try {
                            puppetMasterService.Value.ExecuteFilterRoutingPolicyCommand();
                        }
                        catch (SocketException) {
                            putServiceAside(puppetMasterService.Key);
                        }
                }
            }
            else if (fields.Length == 2 && command.Equals("Ordering")) {
                if (fields[1] == "NO") {
                    foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterService in serviceTable)
                        try {
                            puppetMasterService.Value.ExecuteNoOrderingCommand();
                        }
                        catch (SocketException) {
                            putServiceAside(puppetMasterService.Key);
                        }
                }
                else if (fields[1] == "FIFO") {
                    foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterService in serviceTable)
                        try {
                            puppetMasterService.Value.ExecuteFIFOOrderingCommand();
                        }
                        catch (SocketException) {
                            putServiceAside(puppetMasterService.Key);
                        }
                }
                else if (fields[1] == "TOTAL") {
                    foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterService in serviceTable)
                        try {
                            puppetMasterService.Value.ExecuteTotalOrderingCommand();
                        }
                        catch (SocketException) {
                            putServiceAside(puppetMasterService.Key);
                        }
                }
            }
            else if (fields.Length == 2 && command.Equals("LoggingLevel")) {
                if (fields[1].Equals("full")) {
                    foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterService in serviceTable)
                        try {
                            puppetMasterService.Value.ExecuteFullLoggingLevelCommand();
                        }
                        catch (SocketException) {
                            putServiceAside(puppetMasterService.Key);
                        }
                }
                else if (fields[1].Equals("light")) {
                    foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterService in serviceTable)
                        try {
                            puppetMasterService.Value.ExecuteLightLoggingLevelCommand();
                        }
                        catch (SocketException) {
                            putServiceAside(puppetMasterService.Key);
                        }
                }
            }
            else if (fields.Length == 4 && command.Equals("Site") && fields[2].Equals("Parent")) {
                // Assumption : The site has necessarily a Puppet Master
                IPuppetMasterService puppetMasterService = (IPuppetMasterService)Activator.GetObject(
                    typeof(IPuppetMasterService),
                    "tcp://" + fields[1] + ":" + DEFAULTPORT + "/PuppetMasterURL");
                try {
                    puppetMasterService.ExecuteSiteCommand(fields[1], fields[3]);
                }
                catch (SocketException) {
                    putServiceAside(fields[1]);
                }
                serviceTable.Add(fields[1], puppetMasterService);
            }
            else if (fields.Length == 4 && command.Equals("Subscriber")) {
                if (fields[2].Equals("Subscribe")) {
                    String service;
                    IPuppetMasterService puppetMasterService;
                    if (subscriberTable.TryGetValue(fields[1], out service) &&
                        serviceTable.TryGetValue(service, out puppetMasterService)) {
                        try {
                            puppetMasterService.ExecuteSubscribeCommand(fields[1], fields[3]);
                        }
                        catch (SocketException) {
                            putServiceAside(service);
                        }
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
                        try {
                            puppetMasterService.ExecuteUnsubscribeCommand(fields[1], fields[3]);
                        }
                        catch (SocketException) {
                            putServiceAside(service);
                        }
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
                        try {
                            puppetMasterService.ExecutePublishCommand(
                                    fields[1],
                                    publishTimes,
                                    fields[5],
                                    intervalTimes);
                        }
                        catch (SocketException) {
                            putServiceAside(service);
                        }
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
                        try {
                            puppetMasterService.ExecuteBrokerCommand(
                                    fields[1],
                                    fields[5],
                                    fields[7]);
                            brokerTable.Add(fields[1], fields[5]);
                        }
                        catch (SocketException) {
                            putServiceAside(fields[5]);
                        }
                    }
                    else {
                        System.Console.WriteLine("Site " + fields[5] + " not found.");
                    }
                }
                else if (fields[3].Equals("publisher")) {
                    IPuppetMasterService puppetMasterService;
                    if (serviceTable.TryGetValue(fields[5], out puppetMasterService)) {
                        try {
                            puppetMasterService.ExecutePublisherCommand(
                                    fields[1],
                                    fields[5],
                                    fields[7]);
                            publisherTable.Add(fields[1], fields[5]);
                        }
                        catch (SocketException) {
                            putServiceAside(fields[5]);
                        }
                    }
                    else {
                        System.Console.WriteLine("Site " + fields[5] + " not found.");
                    }
                }
                else if (fields[3].Equals("subscriber")) {
                    IPuppetMasterService puppetMasterService;
                    if (serviceTable.TryGetValue(fields[5], out puppetMasterService)) {
                        try {
                            puppetMasterService.ExecuteSubscriberCommand(
                                    fields[1],
                                    fields[5],
                                    fields[7]);
                            subscriberTable.Add(fields[1], fields[5]);
                        }
                        catch (SocketException) {
                            putServiceAside(fields[5]);
                        }
                    }
                    else {
                        System.Console.WriteLine("Site " + fields[5] + " not found.");
                    }
                }
            }
            return "test";
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
