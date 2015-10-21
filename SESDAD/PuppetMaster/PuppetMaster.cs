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
using SESDAD.CommonTypes;

namespace SESDAD.PuppetMaster {
    ///<summary>
    /// Puppet Master CLI
    ///</summary>
    internal class PuppetMaster {
        //States
        private RoutingPolicyType routingPolicy;
        private OrderingType ordering;
        private LoggingLevelType loggingLevel;
        private bool siteTreeIsDefined;
        //Constants
        private readonly int PORT;
        private readonly String REGEXURL;
        //Tables
        private IDictionary<String, String> parentSiteTable,
                                            siteResolutionCache,
                                            brokerResolutionCache,
                                            publisherResolutionCache,
                                            subscriberResolutionCache;
        private IDictionary<String, IPuppetMasterService> puppetMasterServiceTable;
        ///<summary>
        /// Puppet Master CLI constructor
        ///</summary>
        internal PuppetMaster() {
            siteTreeIsDefined = false;
            routingPolicy = RoutingPolicyType.FLOODING;
            ordering = OrderingType.FIFO;
            loggingLevel = LoggingLevelType.LIGHT;
            PORT = 1000;
            REGEXURL = @"^tcp://([\w\.]+):\d{1,5}/\w+$";
            siteResolutionCache = new Dictionary<String, String>();
            parentSiteTable = new Dictionary<String, String>();
            puppetMasterServiceTable = new Dictionary<String, IPuppetMasterService>();
            brokerResolutionCache = new Dictionary<String, String>();
            publisherResolutionCache = new Dictionary<String, String>();
            subscriberResolutionCache = new Dictionary<String, String>();
        }
        //<summary>
        // Starts connection with Puppet Master Service
        //</summary>
        internal void Connect() {
            TcpChannel channel = new TcpChannel(PORT);
            ChannelServices.RegisterChannel(channel, true);
            PuppetMasterService puppetMasterService = new PuppetMasterService();
            RemotingServices.Marshal(
                puppetMasterService,
                "PuppetMasterService",
                typeof(PuppetMasterService));
        }
        //<summary>
        // Reads configuration file
        //</summary>
        internal void ExecuteConfigurationFile(String configurationFileName) {
            String line;
            String[] fields;
            int waitingTime;
            StreamReader file = new StreamReader(configurationFileName);
            while ((line = file.ReadLine()) != null) {
                System.Console.WriteLine(line);
                fields = line.Split(' ');
                if (fields.Length == 2 &&
                    fields[0].Equals("Wait") &&
                    Int32.TryParse(fields[1], out waitingTime)) {
                    Thread.Sleep(waitingTime);
                }
                ParseLineToCommand(line);
            }
            file.Close();
        }
        //<summary>
        // Creates CLI interface for user interaction with Puppet Master Service
        //</summary>
        internal void StartCLI() {
            String command, reply;
            while (true) {
                System.Console.Write("PuppetMaster> ");
                command = System.Console.ReadLine();
                ParseLineToCommand(command);
            }
        }
        private bool TryGetServiceURL(String processName, out String serviceURL) {
            return (brokerResolutionCache.TryGetValue(processName, out serviceURL) ||
                  publisherResolutionCache.TryGetValue(processName, out serviceURL) ||
                  subscriberResolutionCache.TryGetValue(processName, out serviceURL));
        }
        //<summary>
        // Converts string input into command
        //</summary>
        private void ParseLineToCommand(String line) {
            String command, serviceURL;
            String[] fields;
            fields = line.Split(' ');
            command = fields[0];

            if (!siteTreeIsDefined && fields.Length == 4 && command.Equals("Site") && fields[2].Equals("Parent")) {
                parentSiteTable.Add(fields[1], fields[3]);
            }
            else {
                if (fields.Length == 1 && command.Equals("Status")) {
                    foreach (IPuppetMasterService service in puppetMasterServiceTable.Values.ToList()) {
                        service.ExecuteStatusCommand();
                    }
                }
                else if (fields.Length == 2 &&
                         command.Equals("Crash") &&
                         TryGetServiceURL(fields[1], out serviceURL)) {
                    puppetMasterServiceTable[serviceURL].ExecuteCrashCommand(fields[1]);
                }
                else if (fields.Length == 2 &&
                         command.Equals("Freeze") &&
                         TryGetServiceURL(fields[1], out serviceURL)) {
                    puppetMasterServiceTable[serviceURL].ExecuteFreezeCommand(fields[1]);
                }
                else if (fields.Length == 2 &&
                         command.Equals("Unfreeze") &&
                         TryGetServiceURL(fields[1], out serviceURL)) {
                    puppetMasterServiceTable[serviceURL].ExecuteUnfreezeCommand(fields[1]);
                }
                else if (fields.Length == 2 && command.Equals("RoutingPolicy")) {
                    if (fields[1].Equals("flooding")) {
                        routingPolicy = RoutingPolicyType.FLOODING;
                    }
                    else if (fields[1].Equals("filter")) {
                        routingPolicy = RoutingPolicyType.FILTER;
                    }
                    else {
                        return;
                    }
                    foreach (IPuppetMasterService service in puppetMasterServiceTable.Values.ToList()) {
                        service.RoutingPolicy = routingPolicy;
                    }
                }
                else if (fields.Length == 2 && command.Equals("Ordering")) {
                    if (fields[1].Equals("NO")) {
                        ordering = OrderingType.NO;
                    }
                    else if (fields[1].Equals("FIFO")) {
                        ordering = OrderingType.FIFO;
                    }
                    else if (fields[1].Equals("TOTAL")) {
                        ordering = OrderingType.TOTAL;
                    }
                    else {
                        return;
                    }
                    foreach (IPuppetMasterService service in puppetMasterServiceTable.Values.ToList()) {
                        service.Ordering = ordering;
                    }
                }
                else if (fields.Length == 2 && command.Equals("LoggingLevel")) {
                    if (fields[1].Equals("full")) {
                        loggingLevel = LoggingLevelType.FULL;
                    }
                    else if (fields[1].Equals("light")) {
                        loggingLevel = LoggingLevelType.LIGHT;
                    }
                    else {
                        return;
                    }
                    foreach (IPuppetMasterService service in puppetMasterServiceTable.Values.ToList()) {
                        service.LoggingLevel = loggingLevel;
                    }
                }
                else if (fields.Length == 4 &&
                         command.Equals("Subscriber") &&
                         subscriberResolutionCache.TryGetValue(fields[1], out serviceURL)) {
                    if (fields[2].Equals("Subscribe")) {
                        puppetMasterServiceTable[serviceURL].ExecuteSubscribeCommand(fields[1], fields[3]);
                    }
                    else if (fields[2].Equals("Unsubscribe")) {
                        puppetMasterServiceTable[serviceURL].ExecuteUnsubscribeCommand(fields[1], fields[3]);
                    }
                }
                else if (fields.Length == 8 &&
                         command.Equals("Publisher") &&
                         fields[2].Equals("Publish") &&
                         fields[4].Equals("Ontopic") &&
                         fields[6].Equals("Interval")) {
                    int publishTimes, intervalTimes;
                    if (Int32.TryParse(fields[3], out publishTimes) &&
                        Int32.TryParse(fields[7], out intervalTimes) &&
                        publisherResolutionCache.TryGetValue(fields[1], out serviceURL)) {
                        puppetMasterServiceTable[serviceURL].ExecutePublishCommand(
                                fields[1],
                                publishTimes,
                                fields[5],
                                intervalTimes);
                    }
                }
                else if (fields.Length == 8 && command.Equals("Process") &&
                            fields[2].Equals("Is") &&
                            fields[4].Equals("On") &&
                            fields[6].Equals("URL") &&
                            Regex.IsMatch(fields[7], REGEXURL)) {

                    String processName = fields[1],
                           processType = fields[3],
                           siteName = fields[5],
                           processURI = fields[7];
                    serviceURL = Regex.Match(processURI, REGEXURL).Groups[1].Value;

                    IPuppetMasterService serviceProxy;
                    if (!puppetMasterServiceTable.TryGetValue(serviceURL, out serviceProxy)) {
                        serviceProxy = (IPuppetMasterService)Activator.GetObject(
                               typeof(IPuppetMasterService),
                               @"tcp://" + serviceURL + ":" + PORT + @"/PuppetMasterService");

                        serviceProxy.RoutingPolicy = routingPolicy;
                        serviceProxy.Ordering = ordering;
                        serviceProxy.LoggingLevel = loggingLevel;

                        puppetMasterServiceTable.Add(serviceURL, serviceProxy);
                    }

                    if (processType.Equals("broker")) {
                        String parentSiteName,
                               parentBrokerURL;

                        if (parentSiteTable.TryGetValue(siteName, out parentSiteName)) {
                            siteResolutionCache.TryGetValue(parentSiteName, out parentBrokerURL);
                            if (parentBrokerURL == null) {
                                parentBrokerURL = "none";
                            }

                            puppetMasterServiceTable[serviceURL].ExecuteBrokerCommand(
                                    processName,
                                    siteName,
                                    processURI,
                                    parentBrokerURL);

                            brokerResolutionCache.Add(processName, serviceURL);
                            siteResolutionCache.Add(siteName, processURI);
                        }
                    }
                    else if (processType.Equals("publisher")) {
                        String brokerURL;

                        siteResolutionCache.TryGetValue(siteName, out brokerURL);
                        puppetMasterServiceTable[serviceURL].ExecutePublisherCommand(
                                processName,
                                siteName,
                                processURI,
                                brokerURL);
                        publisherResolutionCache.Add(processName, serviceURL);
                    }
                    else if (processType.Equals("subscriber")) {
                        String brokerURL;

                        siteResolutionCache.TryGetValue(siteName, out brokerURL);
                        puppetMasterServiceTable[serviceURL].ExecuteSubscriberCommand(
                                processName,
                                siteName,
                                processURI,
                                brokerURL);
                        subscriberResolutionCache.Add(processName, serviceURL);
                    }
                }
                else {
                    return;
                }
                siteTreeIsDefined = true;
            }
        }
    }
    //<summary>
    // Project entry point class
    //</summary>
    public static class Program {
        //<summary>
        // Project entry point method
        //</summary>
        public static void Main(string[] args) {
            PuppetMaster puppetMaster = new PuppetMaster();
            puppetMaster.Connect();
            System.Console.WriteLine("Connected to SESDAD.");
            if (args.Length == 1 && File.Exists(args[0])) {
                System.Console.WriteLine("Accessing to configuration file...");
                puppetMaster.ExecuteConfigurationFile(args[0]);
                System.Console.WriteLine("Implemented configuration file");
            }
            else {
                System.Console.WriteLine("Awaiting for instructions...");
            }
            puppetMaster.StartCLI();
        }
    }
}
