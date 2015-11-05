using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.Remoting;
using System.Text.RegularExpressions;

using SESDAD.CommonTypes;

namespace SESDAD.Managing {
    internal enum ProcessType {
        BROKER,
        PUBLISHER,
        SUBSCRIBER
    }
    internal enum ProcessState {
        OFFLINE,
        UNFROZEN,
        FROZEN,
        CRASHED,
        UNDEFINED
    }
    ///<summary>
    /// Puppet Master Service
    ///</summary>
    public class PuppetMasterService : MarshalByRefObject, SESDAD.Managing.IPuppetMasterService, SESDAD.CommonTypes.IPuppetMasterService {
        // Log
        private String log,
            // ID
                       serviceName,
                       serviceURL;
        private int port;
        // States
        private RoutingPolicyType routingPolicy;
        private OrderingType ordering;
        private LoggingLevelType loggingLevel;
        // Constants
        private readonly String BROKERFILE,
                                PUBLISHERFILE,
                                SUBSCRIBERFILE;
        // Tables
        private IDictionary<String, ProcessState> stateList;
        private IDictionary<String, IMessageBrokerService> brokerTable;
        private IDictionary<String, IPublisherService> publisherTable;
        private IDictionary<String, ISubscriberService> subscriberTable;
        ///<summary>
        /// Puppet Master Service constructor
        ///</summary>
        public PuppetMasterService(String newServiceName, int newPort) {
            log = "";
            serviceName = newServiceName;
            serviceURL = "tcp://localhost:" + newPort + "/" + newServiceName;
            port = newPort;
            routingPolicy = RoutingPolicyType.FLOODING;
            ordering = OrderingType.FIFO;
            loggingLevel = LoggingLevelType.LIGHT;
            brokerTable = new Dictionary<String, IMessageBrokerService>();
            publisherTable = new Dictionary<String, IPublisherService>();
            subscriberTable = new Dictionary<String, ISubscriberService>();
            stateList = new Dictionary<String, ProcessState>();
            BROKERFILE = "MessageBroker.exe";
            PUBLISHERFILE = "Publisher.exe";
            SUBSCRIBERFILE = "Subscriber.exe";
        }
        ///<summary>
        /// Puppet Master Service routing policy
        ///</summary>
        public RoutingPolicyType RoutingPolicy {
            set {
                routingPolicy = value;
                foreach (IMessageBrokerService brokerService in brokerTable.Values) {
                    brokerService.RoutingPolicy = value;
                }
            }
        }
        ///<summary>
        /// Puppet Master Service ordering
        ///</summary>
        public OrderingType Ordering {
            set {
                ordering = value;
                foreach (IMessageBrokerService brokerService in brokerTable.Values) {
                    brokerService.Ordering = value;
                }
            }
        }
        ///<summary>
        /// Puppet Master Service logging level
        ///</summary>
        public LoggingLevelType LoggingLevel {
            set {
                loggingLevel = value;
            }
        }
        ///<summary>
        /// Creates a broker process
        ///</summary>
        public void ExecuteBrokerCommand(
            String processName,
            String siteName,
            String processURL,
            String parentBrokerURL) {
            Monitor.Enter(stateList);
            stateList.Add(processName, ProcessState.OFFLINE);
            String arguments = processName + " " + siteName + " " + processURL + " " + parentBrokerURL;
            System.Diagnostics.Process.Start(BROKERFILE, arguments);
            Thread.Sleep(1);
            IMessageBrokerService processService = (IMessageBrokerService)Activator.GetObject(
                typeof(IMessageBrokerService),
                processURL);
            processService.ConnectToPuppetMaster(serviceURL);
            brokerTable.Add(processName, processService);
            stateList[processName] = ProcessState.UNFROZEN;
            Monitor.Pulse(stateList);
            Monitor.Exit(stateList);
        }
        ///<summary>
        /// Creates a publisher process
        ///</summary>
        public void ExecutePublisherCommand(
            String processName,
            String siteName,
            String processURL,
            String brokerURL) {
            Monitor.Enter(stateList);
            stateList.Add(processName, ProcessState.OFFLINE);
            String arguments = processName + " " + siteName + " " + processURL + " " + brokerURL;
            System.Diagnostics.Process.Start(PUBLISHERFILE, arguments);
            Thread.Sleep(1);
            IPublisherService processService = (IPublisherService)Activator.GetObject(
                typeof(IPublisherService),
                processURL);
            processService.ConnectToPuppetMaster(serviceURL);
            publisherTable.Add(processName, processService);
            stateList[processName] = ProcessState.UNFROZEN;
            Monitor.Pulse(stateList);
            Monitor.Exit(stateList);
        }
        ///<summary>
        /// Creates a subscriber process
        ///</summary>
        public void ExecuteSubscriberCommand(
            String processName,
            String siteName,
            String processURL,
            String brokerURL) {
            Monitor.Enter(stateList);
            stateList.Add(processName, ProcessState.OFFLINE);
            String arguments = processName + " " + siteName + " " + processURL + " " + brokerURL;
            System.Diagnostics.Process.Start(SUBSCRIBERFILE, arguments);
            Thread.Sleep(1);
            ISubscriberService processService = (ISubscriberService)Activator.GetObject(
                typeof(ISubscriberService),
                processURL);
            processService.ConnectToPuppetMaster(serviceURL);
            subscriberTable.Add(processName, processService);
            stateList[processName] = ProcessState.UNFROZEN;
            Monitor.Pulse(stateList);
            Monitor.Exit(stateList);
        }
        ///<summary>
        /// Subscribes into a topic
        ///</summary>
        public void ExecuteSubscribeCommand(
            String processName,
            String topicName) {
                WriteIntoLog("Subscriber " + processName + " Subscribe " + topicName);
            if (stateList[processName].Equals(ProcessState.FROZEN) ||
                stateList[processName].Equals(ProcessState.UNFROZEN)) {
                ISubscriberService remoteService;
                if (subscriberTable.TryGetValue(processName, out remoteService)) {
                    remoteService.ForceSubscribe(topicName);
                }
            }
        }
        ///<summary>
        /// Unsubscribes from a topic
        ///</summary>
        public void ExecuteUnsubscribeCommand(
            String processName,
            String topicName) {
                WriteIntoLog("Subscriber " + processName + " Unsubscribe " + topicName);
            if (stateList[processName].Equals(ProcessState.FROZEN) ||
                stateList[processName].Equals(ProcessState.UNFROZEN)) {
                ISubscriberService remoteService;
                if (subscriberTable.TryGetValue(processName, out remoteService)) {
                    remoteService.ForceUnsubscribe(topicName);
                }
            }
        }
        ///<summary>
        /// Publishes a message
        ///</summary>
        public void ExecutePublishCommand(
            String processName,
            int publishTimes,
            String topicName,
            int intervalTime) {
                WriteIntoLog("Publisher " + processName + " Publish " + publishTimes + " Ontopic " + topicName + " Interval " + intervalTime);
            if (stateList[processName].Equals(ProcessState.FROZEN) ||
                stateList[processName].Equals(ProcessState.UNFROZEN)) {
                IPublisherService remoteService;
                if (publisherTable.TryGetValue(processName, out remoteService)) {
                    for (int times = 0; times < publishTimes; times++) {
                        remoteService.ForcePublish(topicName, "Message");
                        Thread.Sleep(intervalTime);
                    }
                }
            }
        }
        ///<summary>
        /// Gets Puppet Master Service status
        ///</summary>
        public void ExecuteStatusCommand() {
            String separator = "--------------------------------------------------------------------";
            String nl = Environment.NewLine;


            Console.WriteLine(separator + nl +
                " Routing Policy : " + routingPolicy + nl +
                " Ordering :       " + ordering + nl +
                " Logging Level :  " + loggingLevel + nl +
                separator + nl +
                " Predicted states :");
            foreach (KeyValuePair<String, ProcessState> process in stateList) {
                Console.WriteLine(" -> " + process.Key + " is " + process.Value);
            }
            Console.WriteLine(separator + nl +
                " Brokers :" + nl);
            foreach (IMessageBrokerService process in brokerTable.Values) {
                Console.WriteLine(process.GetStatus());
            }
            Console.WriteLine(separator + nl +
                " Publishers :" + nl);
            foreach (IPublisherService process in publisherTable.Values) {
                Console.WriteLine(process.GetStatus());
            }
            Console.WriteLine(separator + nl +
                " Subscribers :" + nl);
            foreach (ISubscriberService process in subscriberTable.Values) {
                Console.WriteLine(process.GetStatus());
            }
            Console.WriteLine(separator + nl +
                " Log :" + nl +
                log + nl +
                separator);
        }
        private bool TryGetService(String processName, out IGenericProcessService remoteService) {
            if (brokerTable.ContainsKey(processName)) {
                remoteService = brokerTable[processName];
            }
            else if (publisherTable.ContainsKey(processName)) {
                remoteService = publisherTable[processName];
            }
            else if (subscriberTable.ContainsKey(processName)) {
                remoteService = subscriberTable[processName];
            }
            else {
                remoteService = null;
                return false;
            }
            return true;
        }
        ///<summary>
        /// Crashes a process
        ///</summary>
        public void ExecuteCrashCommand(String processName) {
            WriteIntoLog("Crash " + processName);
            if (stateList[processName].Equals(ProcessState.FROZEN) ||
                stateList[processName].Equals(ProcessState.UNFROZEN)) {
                IGenericProcessService remoteService;
                if (TryGetService(processName, out remoteService)) {
                    remoteService.ForceCrash();
                    stateList[processName] = ProcessState.CRASHED;
                }
            }
        }
        ///<summary>
        /// Freezes a process
        ///</summary>
        public void ExecuteFreezeCommand(String processName) {
            WriteIntoLog("Freeze " + processName);
            if (stateList[processName].Equals(ProcessState.UNFROZEN)) {
                IGenericProcessService remoteService;
                if (TryGetService(processName, out remoteService)) {
                    remoteService.ForceFreeze();
                    stateList[processName] = ProcessState.FROZEN;
                }
            }
        }
        ///<summary>
        /// Unfreezes a process
        ///</summary>
        public void ExecuteUnfreezeCommand(String processName) {
            WriteIntoLog("Unfreeze " + processName);
            if (stateList[processName].Equals(ProcessState.FROZEN)) {
                IGenericProcessService remoteService;
                if (TryGetService(processName, out remoteService)) {
                    remoteService.ForceUnfreeze();
                    stateList[processName] = ProcessState.UNFROZEN;
                }
            }
        }
        ///<summary>
        /// Writes into the Puppet Master Service Log while in full log
        ///</summary>
        public void WriteIntoFullLog(String logMessage) {
            if (loggingLevel == LoggingLevelType.FULL) {
                WriteIntoLog(logMessage);
            }
        }
        ///<summary>
        /// Writes into the Puppet Master Service Log
        ///</summary>
        public void WriteIntoLog(String logMessage) {
            log += "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + logMessage + Environment.NewLine;
        }
    }
}
