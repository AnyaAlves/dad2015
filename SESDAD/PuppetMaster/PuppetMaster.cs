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
        private readonly String REGEXURI,
                                REGEXURL;
        //Tables
        private IDictionary<String, String> siteNameToBrokerURLTable,
                                            siteNameToParentSiteNameTable;
        private IDictionary<String, IPuppetMasterService> machineURLToServiceTable,
                                                          publisherNameToServiceTable,
                                                          subscriberNameToServiceTable;
        ///<summary>
        /// Puppet Master CLI constructor
        ///</summary>
        internal PuppetMaster() {
            routingPolicy = RoutingPolicyType.FLOODING;
            ordering = OrderingType.FIFO;
            loggingLevel = LoggingLevelType.LIGHT;
            PORT = 1000;
            REGEXURI = @"^tcp://[\w\.]+:\d{1,5}/\w+$";
            REGEXURL = @"^tcp://([\w\.]+):\d{1,5}/\w+$";
            siteNameToBrokerURLTable = new Dictionary<String, String>();
            siteNameToParentSiteNameTable = new Dictionary<String, String>();
            siteNameToBrokerURLTable.Add("none", "none");
            machineURLToServiceTable = new Dictionary<String, IPuppetMasterService>();
            publisherNameToServiceTable = new Dictionary<String, IPuppetMasterService>();
            subscriberNameToServiceTable = new Dictionary<String, IPuppetMasterService>();
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
            String line, reply;
            StreamReader file = new StreamReader(configurationFileName);
            while ((line = file.ReadLine()) != null) {
                System.Console.WriteLine(line);
                reply = parseLineToCommand(line);
                System.Console.WriteLine(reply);
            }
            file.Close();
        }
        //<summary>
        // Creates CLI interface for user interaction with Puppet Master Service
        //</summary>
        internal void StartCLI() {
            String command, reply;
            while (true) {
                command = System.Console.ReadLine();
                reply = parseLineToCommand(command);
                System.Console.WriteLine(reply);
            }
        }

        private void TryGetService(String process, out IPuppetMasterService service) {
            if (!(publisherNameToServiceTable.TryGetValue(process, out service) ||
                  subscriberNameToServiceTable.TryGetValue(process, out service))) {
                throw new ArgumentNullException(process, "Process not found.");
            };
        }
        //<summary>
        // Converts string input into command
        //</summary>
        private String parseLineToCommand(String line) {
            String[] fields = line.Split(' ');
            String command = fields[0];
            IPuppetMasterService puppetMasterService;

            try {
                if (fields.Length == 1 && command.Equals("Status")) {
                    String status;
                    foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterKeyValuePair in machineURLToServiceTable.ToList()) {
                        puppetMasterKeyValuePair.Value.ExecuteStatusCommand();
                    }
                }
                else if (fields.Length == 2 && command.Equals("Crash")) {
                    TryGetService(fields[1], out puppetMasterService);
                    puppetMasterService.ExecuteCrashCommand(fields[1]);
                }
                else if (fields.Length == 2 && command.Equals("Freeze")) {
                    TryGetService(fields[1], out puppetMasterService);
                    puppetMasterService.ExecuteFreezeCommand(fields[1]);
                }
                else if (fields.Length == 2 && command.Equals("Unfreeze")) {
                    TryGetService(fields[1], out puppetMasterService);
                    puppetMasterService.ExecuteUnfreezeCommand(fields[1]);
                }
                else if (fields.Length == 2 && command.Equals("Wait")) {
                    int integerTime;
                    if (Int32.TryParse(fields[1], out integerTime)) {
                        foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterKeyValuePair in machineURLToServiceTable.ToList()) {
                            puppetMasterKeyValuePair.Value.ExecuteWaitCommand(integerTime);
                        }
                    }
                }
                else if (fields.Length == 2 && command.Equals("RoutingPolicy")) {
                    if (fields[1] == "flooding") {
                        routingPolicy = RoutingPolicyType.FLOODING;
                    }
                    else if (fields[1] == "filter") {
                        routingPolicy = RoutingPolicyType.filter;
                    }
                    foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterKeyValuePair in machineURLToServiceTable.ToList()) {
                        puppetMasterKeyValuePair.Value.RoutingPolicy = routingPolicy;
                    }
                }
                else if (fields.Length == 2 && command.Equals("Ordering")) {
                    if (fields[1] == "NO") {
                        ordering = OrderingType.NO;
                    }
                    else if (fields[1] == "FIFO") {
                        ordering = OrderingType.FIFO;
                    }
                    else if (fields[1] == "TOTAL") {
                        ordering = OrderingType.TOTAL;
                    }
                    foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterKeyValuePair in machineURLToServiceTable.ToList()) {
                        puppetMasterKeyValuePair.Value.Ordering = ordering;
                    }
                }
                else if (fields.Length == 2 && command.Equals("LoggingLevel")) {
                    if (fields[1].Equals("full")) {
                        loggingLevel = LoggingLevelType.full;
                    }
                    else if (fields[1].Equals("light")) {
                        loggingLevel = LoggingLevelType.LIGHT;
                    }
                    foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterKeyValuePair in machineURLToServiceTable.ToList()) {
                        puppetMasterKeyValuePair.Value.LoggingLevel = loggingLevel;
                    }
                }
                else if (fields.Length == 4 && command.Equals("Site") && fields[2].Equals("Parent")) {
                    siteNameToParentSiteNameTable.Add(fields[1], fields[3]);
                }
                else if (fields.Length == 4 && command.Equals("Subscriber")) {
                    puppetMasterService = subscriberNameToServiceTable[fields[1]];
                    if (fields[2].Equals("Subscribe")) {
                        puppetMasterService.ExecuteSubscribeCommand(fields[1], fields[3]);
                    }
                    else if (fields[2].Equals("Unsubscribe")) {
                        puppetMasterService.ExecuteUnsubscribeCommand(fields[1], fields[3]);
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
                        puppetMasterService = publisherNameToServiceTable[fields[1]];
                        puppetMasterService.ExecutePublishCommand(
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
                            Regex.IsMatch(fields[7], REGEXURI)) {

                    String processName = fields[1],
                           processType = fields[3],
                           siteName = fields[5],
                           processURI = fields[7],
                           puppetMasterServiceURL = Regex.Match(processURI, REGEXURL).Groups[1].Value;

                    if (!machineURLToServiceTable.TryGetValue(puppetMasterServiceURL, out puppetMasterService)) {
                        puppetMasterService = (IPuppetMasterService)Activator.GetObject(
                               typeof(IPuppetMasterService),
                               @"tcp://" + puppetMasterServiceURL + ":" + PORT + @"/PuppetMasterService");

                        puppetMasterService.RoutingPolicy = routingPolicy;
                        puppetMasterService.Ordering = ordering;
                        puppetMasterService.LoggingLevel = loggingLevel;

                        machineURLToServiceTable.Add(puppetMasterServiceURL, puppetMasterService);
                    }

                    if (processType.Equals("broker")) {
                        String parentSiteName = siteNameToParentSiteNameTable[siteName],
                               parentBrokerURI = siteNameToBrokerURLTable[parentSiteName];

                        puppetMasterService.ExecuteBrokerCommand(
                                processName,
                                siteName,
                                processURI,
                                parentBrokerURI);

                        siteNameToBrokerURLTable.Add(siteName, processURI);
                    }
                    else if (processType.Equals("publisher")) {
                        puppetMasterService.ExecutePublisherCommand(
                                processName,
                                siteName,
                                processURI);
                        publisherNameToServiceTable.Add(processName, puppetMasterService);
                    }
                    else if (processType.Equals("subscriber")) {
                        puppetMasterService.ExecuteSubscriberCommand(
                                processName,
                                siteName,
                                processURI);
                        subscriberNameToServiceTable.Add(processName, puppetMasterService);
                    }
                }
                else {
                    return "Invalid command";
                }
            }
            catch (ArgumentNullException e) {
                return e.Message;
            }
            catch (SocketException e) {
                return e.Message;
            }
            catch (RemotingException e) {
                return e.Message;
            }
            catch (KeyNotFoundException e) {
                return e.Message;
            }
            return "Done";
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
            System.Console.WriteLine("Connected to PuppetMasterURL");
            if (args.Length == 1 && File.Exists(args[0])) {
                puppetMaster.ExecuteConfigurationFile(args[0]);
                puppetMaster.StartCLI();
            }
            System.Console.ReadLine();
        }
    }
}
