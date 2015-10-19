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
        //States
        private RoutingPolicyType routingPolicy;
        private OrderingType ordering;
        private LoggingLevelType loggingLevel;
        //Constants
        private const int PORT = 1000;
        private const String REGEXURI = @"^tcp://[\w\.]+:\d{1,5}/\w+$",
                             REGEXURL = @"^tcp://([\w\.])+:\d{1,5}/\w+$";
        //Tables
        private IDictionary<String, String> siteNameToBrokerURLTable,
                                            siteNameToParentSiteNameTable;
        private IDictionary<String, IPuppetMasterService> machineURLToPuppetMasterServiceTable,
                                                          publisherNameToPuppetMasterServiceTable,
                                                          subscriberNameToPuppetMasterServiceTable;

        public PuppetMaster() {
            routingPolicy = RoutingPolicyType.flooding;
            ordering = OrderingType.FIFO;
            loggingLevel = LoggingLevelType.light;
            siteNameToBrokerURLTable = new Dictionary<String, String>();
            siteNameToParentSiteNameTable = new Dictionary<String, String>();
            siteNameToBrokerURLTable.Add("none", null);
            machineURLToPuppetMasterServiceTable = new Dictionary<String, IPuppetMasterService>();
            publisherNameToPuppetMasterServiceTable = new Dictionary<String, IPuppetMasterService>();
            subscriberNameToPuppetMasterServiceTable = new Dictionary<String, IPuppetMasterService>();
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

        //<summary>
        // Read configuration file
        //</summary>
        public void ExecuteConfigurationFile(String configurationFileName) {
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

        private void TryGetService(String process, out IPuppetMasterService service) {
            if (!(publisherNameToPuppetMasterServiceTable.TryGetValue(process, out service) ||
                  subscriberNameToPuppetMasterServiceTable.TryGetValue(process, out service))) {
                throw new ArgumentNullException(process, "Process not found.");
            };
        }

        //<summary>
        // Identify command
        //</summary>
        private String parseLineToCommand(String line) {
            String[] fields = line.Split(' ');
            String command = fields[0];
            IPuppetMasterService puppetMasterService;

            try {
                if (fields.Length == 1 && command.Equals("Status")) {
                    foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterKeyValuePair in machineURLToPuppetMasterServiceTable) {
                        return puppetMasterKeyValuePair.Value.ExecuteStatusCommand();
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
                        foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterKeyValuePair in machineURLToPuppetMasterServiceTable) {
                            puppetMasterKeyValuePair.Value.ExecuteWaitCommand(integerTime);
                        }
                    }
                }
                else if (fields.Length == 2 && command.Equals("RoutingPolicy")) {
                    if (fields[1] == "flooding") {
                        routingPolicy = RoutingPolicyType.flooding;
                    }
                    else if (fields[1] == "filter") {
                        routingPolicy = RoutingPolicyType.filter;
                    }
                    foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterKeyValuePair in machineURLToPuppetMasterServiceTable) {
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
                    foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterKeyValuePair in machineURLToPuppetMasterServiceTable) {
                        puppetMasterKeyValuePair.Value.Ordering = ordering;
                    }
                }
                else if (fields.Length == 2 && command.Equals("LoggingLevel")) {
                    if (fields[1].Equals("full")) {
                        loggingLevel = LoggingLevelType.full;
                    }
                    else if (fields[1].Equals("light")) {
                        loggingLevel = LoggingLevelType.light;
                    }
                    foreach (KeyValuePair<String, IPuppetMasterService> puppetMasterKeyValuePair in machineURLToPuppetMasterServiceTable) {
                        puppetMasterKeyValuePair.Value.LoggingLevel = loggingLevel;
                    }
                }
                else if (fields.Length == 4 && command.Equals("Site") && fields[2].Equals("Parent")) {
                    siteNameToParentSiteNameTable.Add(fields[1], fields[3]);
                }
                else if (fields.Length == 4 && command.Equals("Subscriber")) {
                    subscriberNameToPuppetMasterServiceTable.TryGetValue(fields[1], out puppetMasterService);
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
                        publisherNameToPuppetMasterServiceTable.TryGetValue(fields[1], out puppetMasterService);
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
                            new Regex(REGEXURI).IsMatch(fields[7])) {

                    String processName = fields[1],
                           processType = fields[3],
                           siteName = fields[5],
                           processURI = fields[7],
                           puppetMasterServiceURL = Regex.Match(processURI, REGEXURL).Groups[1].Value;

                    if (!machineURLToPuppetMasterServiceTable.TryGetValue(puppetMasterServiceURL, out puppetMasterService)) {
                        puppetMasterService = (IPuppetMasterService)Activator.GetObject(
                               typeof(IPuppetMasterService),
                               @"tcp://" + puppetMasterServiceURL + ":" + PORT + @"/PuppetMasterService");

                        puppetMasterService.RoutingPolicy = routingPolicy;
                        puppetMasterService.Ordering = ordering;
                        puppetMasterService.LoggingLevel = loggingLevel;

                        machineURLToPuppetMasterServiceTable.Add(puppetMasterServiceURL, puppetMasterService);
                    }

                    if (processType.Equals("broker")) {
                        String parentSiteName,
                               parentBrokerURI;

                        if (!siteNameToParentSiteNameTable.TryGetValue(siteName, out parentSiteName)) {
                            throw new ArgumentNullException(siteName, "Parent site not found.");
                        }
                        if (!siteNameToBrokerURLTable.TryGetValue(parentSiteName, out parentBrokerURI)) {
                            throw new ArgumentNullException(parentSiteName, "Broker URI not found.");
                        }

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
                        publisherNameToPuppetMasterServiceTable.Add(processName, puppetMasterService);
                    }
                    else if (processType.Equals("subscriber")) {
                        puppetMasterService.ExecuteSubscriberCommand(
                                processName,
                                siteName,
                                processURI);
                        subscriberNameToPuppetMasterServiceTable.Add(processName, puppetMasterService);
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
            return "Done";
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
                puppetMaster.ExecuteConfigurationFile(args[0]);
                puppetMaster.StartCLI();
            }
            System.Console.ReadLine();
        }
    }
}
