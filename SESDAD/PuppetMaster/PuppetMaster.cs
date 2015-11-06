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
        private const int PORT = 30000;
        private const String REGEXURL = @"^tcp://([\w\.]+):\d{1,5}/\w+$";
        private const String SERVICE_NAME = "puppet";
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
            routingPolicy = RoutingPolicyType.FLOOD;
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
            PuppetMasterService puppetMasterService = new PuppetMasterService(SERVICE_NAME, PORT);
            RemotingServices.Marshal(
                puppetMasterService,
                SERVICE_NAME,
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

            routingPolicy = RoutingPolicyType.FLOOD;
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
            StreamReader file = new StreamReader(configurationFileName);

            while ((line = file.ReadLine()) != null) {
                System.Console.WriteLine(line);
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
            try {
                int waitingTime;
                String serviceURL;
                String[] fields = line.Split(' ');
                String command = fields[0];
                if (fields.Length == 2 &&
                    command.ToLower().Equals("Wait") &&
                    Int32.TryParse(fields[1], out waitingTime)) {
                    Thread.Sleep(waitingTime);
                }
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
                else if (fields.Length == 2 && command.ToLower().Equals("routingpolicy")) {
                    if (fields[1].ToLower().Equals("flooding")) {
                        routingPolicy = RoutingPolicyType.FLOOD;
                    }
                    else if (fields[1].ToLower().Equals("filter")) {
                        routingPolicy = RoutingPolicyType.FILTER;
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
                               @"tcp://" + serviceURL + ":" + PORT + "/" + SERVICE_NAME);

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

        private string GetScriptsDir() {
            string dir = Directory.GetCurrentDirectory();
            DirectoryInfo dirInfo = null;
            //go 2 directories up
            for (int i = 0; i < 2; i++) {
                dirInfo = Directory.GetParent(dir);
                dir = dirInfo.FullName;
            }
            return dirInfo.GetDirectories("Scripts").First().FullName;
        }

        public void Run(String fileNames) {
            System.Console.Clear();
            string dir = GetScriptsDir();

            foreach (String fileName in fileNames.Split(' ')) {
                if (!File.Exists(dir + "\\" + fileName)) {
                    //get a list of scripts inside the folder
                    Console.WriteLine("Available scripts: ");
                    foreach (string file in Directory.GetFiles(dir)) {
                        Console.WriteLine(" " + Path.GetFileName(file));
                    }
                    return;
                }
                ExecuteConfigurationFile(dir + "\\" + fileName);
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
            String fileNames = string.Join("", args);

            // Close all processes when ctrl+c is pressed
            Console.CancelKeyPress += new ConsoleCancelEventHandler(puppetMaster.CloseProcesses);

            puppetMaster.LaunchService();

            while (true) {
                puppetMaster.Run(fileNames);
                fileNames = Console.ReadLine();
            }
        }
    }
}
