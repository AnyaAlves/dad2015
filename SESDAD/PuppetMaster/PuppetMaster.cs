

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
        //martelos do pooky
        private bool siteTreeIsDefined;
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
            siteTreeIsDefined = false; //martelo aqui
            routingPolicy = RoutingPolicyType.FLOODING;
            ordering = OrderingType.FIFO;
            loggingLevel = LoggingLevelType.LIGHT;
            PORT = 1000;
            REGEXURL = @"^tcp://([\w\.]+):\d{1,5}/\w+$";
            SERVICENAME = "PuppetMasterService";
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
        // Reads configuration file
        //</summary>
        internal void ExecuteConfigurationFile(String configurationFileName) {
            String line;
            String[] fields;
            int waitingTime;
            StreamReader file = new StreamReader(configurationFileName);
            while ((line = file.ReadLine()) != null) {
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
            String command;
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
                Site parentSite;
                siteTable.TryGetValue(fields[3], out parentSite);
                siteTable.Add(fields[1], new Site(fields[1], parentSite));
            }
            else {
                if (fields.Length == 1 && command.Equals("Status")) {
                    foreach (IPuppetMasterService service in puppetMasterServiceTable.Values) {
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
                        routingPolicy = RoutingPolicyType.FILTERING;
                    }
                    else {
                        return;
                    }
                    foreach (IPuppetMasterService service in puppetMasterServiceTable.Values) {
                        service.RoutingPolicy = routingPolicy;
                    }
                }
                else if (fields.Length == 2 && command.Equals("Ordering")) {
                    if (fields[1].Equals("NO")) {
                        ordering = OrderingType.NO_ORDER;
                    }
                    else if (fields[1].Equals("FIFO")) {
                        ordering = OrderingType.FIFO;
                    }
                    else if (fields[1].Equals("TOTAL")) {
                        ordering = OrderingType.TOTAL_ORDER;
                    }
                    else {
                        return;
                    }
                    foreach (IPuppetMasterService service in puppetMasterServiceTable.Values) {
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
                    foreach (IPuppetMasterService service in puppetMasterServiceTable.Values) {
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
                else if (fields.Length == 8 &&
                            command.Equals("Process") &&
                            fields[2].Equals("Is") &&
                            fields[4].Equals("On") &&
                            fields[6].Equals("URL") &&
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

                    if (processType.Equals("broker")) {
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
                    else if (processType.Equals("publisher")) {
                        Site site;

                        siteTable.TryGetValue(siteName, out site);
                        puppetMasterServiceTable[serviceURL].ExecutePublisherCommand(
                                processName,
                                siteName,
                                processURL,
                                site.BrokerURL);
                        publisherResolutionCache.Add(processName, serviceURL);
                    }
                    else if (processType.Equals("subscriber")) {
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
            FileAttributes attr;
            String[] filepaths;

            puppetMaster.LaunchService();
            foreach (String arg in args) {
                attr = File.GetAttributes(arg);

                if (attr.HasFlag(FileAttributes.Directory)) {
                    filepaths = Directory.GetFiles(arg);
                }
                else {
                    filepaths = new[] { arg };
                }

                foreach (String filepath in filepaths) {
                    puppetMaster.ExecuteConfigurationFile(filepath);
                    puppetMaster.StartCLI();
                }
            }
            puppetMaster.StartCLI();
        }
    }
}
