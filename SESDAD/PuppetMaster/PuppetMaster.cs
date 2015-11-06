

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

namespace SESDAD.Managing {
    ///<summary>
    /// Puppet Master CLI
    ///</summary>
    internal class PuppetMaster {
        //States
        private RoutingPolicyType routingPolicy;
        private OrderingType ordering;
        private LoggingLevelType loggingLevel;
        //Constants
        private readonly int PORT;
        private readonly String REGEXURL,
                             SERVICENAME;
        //Tables
        private IDictionary<String, Site> siteTable;
        private IDictionary<String, String> brokerResolutionCache,
                                            publisherResolutionCache,
                                            subscriberResolutionCache;
        private IDictionary<String, IPuppetMasterService> puppetMasterServiceTable;
        ///<summary>
        /// Puppet Master CLI constructor
        ///</summary>
        internal PuppetMaster() {
            PORT = 1000;
            REGEXURL = @"^tcp://([\w\.]+):\d{1,5}/\w+$";
            SERVICENAME = "PuppetMasterService";

            routingPolicy = RoutingPolicyType.FLOODING;
            ordering = OrderingType.FIFO;
            loggingLevel = LoggingLevelType.LIGHT;

            siteTable = new Dictionary<String, Site>();

            puppetMasterServiceTable = new Dictionary<String, IPuppetMasterService>();

            brokerResolutionCache = new Dictionary<String, String>();
            publisherResolutionCache = new Dictionary<String, String>();
            subscriberResolutionCache = new Dictionary<String, String>();
        }
        internal void TcpConnect() {
            TcpChannel channel = new TcpChannel(PORT);
            ChannelServices.RegisterChannel(channel, true);
        }

        //<summary>
        // Starts connection with Puppet Master Service
        //</summary>
        internal void LaunchService() {
            TcpConnect();
            PuppetMasterService puppetMasterService = new PuppetMasterService(SERVICENAME, PORT);
            RemotingServices.Marshal(
                puppetMasterService,
                "PuppetMasterService",
                typeof(PuppetMasterService));
        }

        //<summary>
        // Close processes
        //</summary>
        internal void CloseProcesses(object sender, ConsoleCancelEventArgs args) {
            CloseProcesses();
        }

        //<summary>
        // Close processes
        //</summary>
        internal void CloseProcesses() {
            foreach (IPuppetMasterService service in puppetMasterServiceTable.Values) {
                service.CloseProcesses();
            }

            routingPolicy = RoutingPolicyType.FLOODING;
            ordering = OrderingType.FIFO;
            loggingLevel = LoggingLevelType.LIGHT;

            siteTable = new Dictionary<String, Site>();

            puppetMasterServiceTable = new Dictionary<String, IPuppetMasterService>();

            brokerResolutionCache = new Dictionary<String, String>();
            publisherResolutionCache = new Dictionary<String, String>();
            subscriberResolutionCache = new Dictionary<String, String>();
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
                    continue;
                }
                ParseLineToCommand(line);
            }
            file.Close();
        }
        //<summary>
        // Creates CLI interface for user interaction with Puppet Master Service
        //</summary>
        internal void StartCLI() {
            String command;
            while (true) {
                System.Console.Write("PuppetMaster> ");
                command = System.Console.ReadLine();
                if (!ParseLineToCommand(command)) {
                    break;
                }
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
        private bool ParseLineToCommand(String line) {
            String command, serviceURL;
            String[] fields;
            fields = line.Split(' ');
            command = fields[0];

            try {

                if (fields.Length == 4 && command.ToLower().Equals("site") && fields[2].ToLower().Equals("parent")) {
                    Site parentSite;
                    siteTable.TryGetValue(fields[3], out parentSite);
                    siteTable.Add(fields[1], new Site(fields[1], parentSite));
                }
                else if (fields.Length == 1 && command.ToLower().Equals("status")) {
                    foreach (IPuppetMasterService service in puppetMasterServiceTable.Values) {
                        service.ExecuteStatusCommand();
                    }
                }
                else if (fields.Length == 2 &&
                         command.ToLower().Equals("crash") &&
                         TryGetServiceURL(fields[1], out serviceURL)) {
                    puppetMasterServiceTable[serviceURL].ExecuteCrashCommand(fields[1]);
                }
                else if (fields.Length == 2 &&
                         command.ToLower().Equals("freeze") &&
                         TryGetServiceURL(fields[1], out serviceURL)) {
                    puppetMasterServiceTable[serviceURL].ExecuteFreezeCommand(fields[1]);
                }
                else if (fields.Length == 2 &&
                         command.ToLower().Equals("unfreeze") &&
                         TryGetServiceURL(fields[1], out serviceURL)) {
                    puppetMasterServiceTable[serviceURL].ExecuteUnfreezeCommand(fields[1]);
                }
                else if (fields.Length == 2 && command.ToLower().Equals("routingPolicy")) {
                    if (fields[1].ToLower().Equals("flooding")) {
                        routingPolicy = RoutingPolicyType.FLOODING;
                    }
                    else if (fields[1].ToLower().Equals("filter")) {
                        routingPolicy = RoutingPolicyType.FILTERING;
                    }
                    else {
                        return false;
                    }
                    foreach (IPuppetMasterService service in puppetMasterServiceTable.Values) {
                        service.RoutingPolicy = routingPolicy;
                    }
                }
                else if (fields.Length == 2 && command.ToLower().Equals("ordering")) {
                    if (fields[1].ToLower().Equals("no")) {
                        ordering = OrderingType.NO_ORDER;
                    }
                    else if (fields[1].ToLower().Equals("fifo")) {
                        ordering = OrderingType.FIFO;
                    }
                    else if (fields[1].ToLower().Equals("total")) {
                        ordering = OrderingType.TOTAL_ORDER;
                    }
                    else {
                        return false;
                    }
                    foreach (IPuppetMasterService service in puppetMasterServiceTable.Values) {
                        service.Ordering = ordering;
                    }
                }
                else if (fields.Length == 2 && command.ToLower().Equals("logginglevel")) {
                    if (fields[1].ToLower().Equals("full")) {
                        loggingLevel = LoggingLevelType.FULL;
                    }
                    else if (fields[1].ToLower().Equals("light")) {
                        loggingLevel = LoggingLevelType.LIGHT;
                    }
                    else {
                        return false;
                    }
                    foreach (IPuppetMasterService service in puppetMasterServiceTable.Values) {
                        service.LoggingLevel = loggingLevel;
                    }
                }
                else if (fields.Length == 4 &&
                         command.ToLower().Equals("subscriber") &&
                         subscriberResolutionCache.TryGetValue(fields[1], out serviceURL)) {
                    if (fields[2].ToLower().Equals("subscribe")) {
                        puppetMasterServiceTable[serviceURL].ExecuteSubscribeCommand(fields[1], fields[3]);
                    }
                    else if (fields[2].ToLower().Equals("unsubscribe")) {
                        puppetMasterServiceTable[serviceURL].ExecuteUnsubscribeCommand(fields[1], fields[3]);
                    }
                }
                else if (fields.Length == 8 &&
                         command.ToLower().Equals("publisher") &&
                         fields[2].ToLower().Equals("publish") &&
                         fields[4].ToLower().Equals("ontopic") &&
                         fields[6].ToLower().Equals("interval")) {
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
                else if (fields.Length == 8 &&
                            command.ToLower().Equals("process") &&
                            fields[2].ToLower().Equals("is") &&
                            fields[4].ToLower().Equals("on") &&
                            fields[6].ToLower().Equals("url") &&
                            Regex.IsMatch(fields[7], REGEXURL)) {

                    String processName = fields[1],
                           processType = fields[3],
                           siteName = fields[5],
                           processURL = fields[7];
                    serviceURL = Regex.Match(processURL, REGEXURL).Groups[1].Value;

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

                    if (processType.ToLower().Equals("broker")) {
                        Site site = siteTable[siteName],
                             parentSite;

                        siteTable.TryGetValue(siteName, out parentSite);
                        puppetMasterServiceTable[serviceURL].ExecuteBrokerCommand(
                                processName,
                                siteName,
                                processURL,
                                parentSite.ParentBrokerURL);

                        brokerResolutionCache.Add(processName, serviceURL);
                        site.BrokerURL = processURL;
                    }
                    else if (processType.ToLower().Equals("publisher")) {
                        Site site;

                        siteTable.TryGetValue(siteName, out site);
                        puppetMasterServiceTable[serviceURL].ExecutePublisherCommand(
                                processName,
                                siteName,
                                processURL,
                                site.BrokerURL);
                        publisherResolutionCache.Add(processName, serviceURL);
                    }
                    else if (processType.ToLower().Equals("subscriber")) {
                        Site site;

                        siteTable.TryGetValue(siteName, out site);
                        puppetMasterServiceTable[serviceURL].ExecuteSubscriberCommand(
                                processName,
                                siteName,
                                processURL,
                                site.BrokerURL);
                        subscriberResolutionCache.Add(processName, serviceURL);
                    }
                }
                else {
                    return false;
                }
            }
            catch (Exception e) {
                CloseProcesses();
                throw e;
            }

            return true;
        }
        public void Run(String arguments) {
            System.Console.Clear();
            foreach (String arg in arguments.Split(' ')) {
                if (!File.Exists(arg)) {
                    Console.WriteLine(String.Join(Environment.NewLine, Directory.GetFiles(Directory.GetCurrentDirectory(), "*.txt", SearchOption.AllDirectories)));
                    return;
                }
                ExecuteConfigurationFile(arg);
            }
            StartCLI();
            CloseProcesses();
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
            String filename;

            // Close all processes when ctrl+c is pressed
            Console.CancelKeyPress += new ConsoleCancelEventHandler(puppetMaster.CloseProcesses);

            puppetMaster.LaunchService();
            foreach (String arg in args) {
                puppetMaster.Run(arg);
            }

            while (true) {
                Console.WriteLine("Select File: ");
                filename = Console.ReadLine();
                puppetMaster.Run(filename);
            }
        }
    }
}
