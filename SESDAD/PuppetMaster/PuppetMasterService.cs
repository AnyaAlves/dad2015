using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.Remoting;
using System.Text.RegularExpressions;
using System.Diagnostics;
using SESDAD.Commons;
using SESDAD.Managing.Exceptions;

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
    public class PuppetMasterService : MarshalByRefObject, SESDAD.Managing.IPuppetMasterService, SESDAD.Commons.IPuppetMasterService {
        // ID
        private String serviceName,
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
            serviceName = newServiceName;
            serviceURL = "tcp://localhost:" + newPort + "/" + newServiceName;
            port = newPort;
            routingPolicy = RoutingPolicyType.FLOOD;
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
                try {
                    foreach (IMessageBrokerService brokerService in brokerTable.Values) {
                        brokerService.RoutingPolicy = value;
                    }
                }
                catch (Exception e) {
                    throw e;
                }
            }
        }
        ///<summary>
        /// Puppet Master Service ordering
        ///</summary>
        public OrderingType Ordering {
            set {
                ordering = value;
                try {
                    foreach (IMessageBrokerService brokerService in brokerTable.Values) {
                        brokerService.Ordering = value;
                    }
                }
                catch (Exception e) {
                    throw e;
                }
            }
        }
        ///<summary>
        /// Puppet Master Service logging level
        ///</summary>
        public LoggingLevelType LoggingLevel {
            set {
                try {
                    loggingLevel = value;
                }
                catch (Exception e) {
                    throw e;
                }
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
            lock (stateList) {
                stateList.Add(processName, ProcessState.OFFLINE);
                String arguments = processName + " " + siteName + " " + processURL + " " + parentBrokerURL;
                Process broker = Process.Start(BROKERFILE, arguments);
                broker.Attach(); //DebugTools
                Thread.Sleep(1);
                try {
                    IMessageBrokerService processService = (IMessageBrokerService)Activator.GetObject(
                        typeof(IMessageBrokerService),
                        processURL);
                    processService.ConnectToPuppetMaster(serviceURL);
                    brokerTable.Add(processName, processService);
                }
                catch (Exception e) {
                    throw e;
                }
                stateList[processName] = ProcessState.UNFROZEN;
            }
        }
        ///<summary>
        /// Creates a publisher process
        ///</summary>
        public void ExecutePublisherCommand(
            String processName,
            String siteName,
            String processURL,
            String brokerURL) {
            lock (stateList) {
                stateList.Add(processName, ProcessState.OFFLINE);
                String arguments = processName + " " + siteName + " " + processURL + " " + brokerURL;
                Process publisher = Process.Start(PUBLISHERFILE, arguments);
                publisher.Attach(); //DebugTools
                Thread.Sleep(1);
                try {
                    IPublisherService processService = (IPublisherService)Activator.GetObject(
                        typeof(IPublisherService),
                        processURL);
                    processService.ConnectToPuppetMaster(serviceURL);
                    publisherTable.Add(processName, processService);
                }
                catch (Exception e) {
                    throw e;
                }
                stateList[processName] = ProcessState.UNFROZEN;
            }
        }
        ///<summary>
        /// Creates a subscriber process
        ///</summary>
        public void ExecuteSubscriberCommand(
            String processName,
            String siteName,
            String processURL,
            String brokerURL) {
            lock (stateList) {
                stateList.Add(processName, ProcessState.OFFLINE);
                String arguments = processName + " " + siteName + " " + processURL + " " + brokerURL;
                Process subscriber = Process.Start(SUBSCRIBERFILE, arguments);
                subscriber.Attach(); //DebugTools
                Thread.Sleep(1);
                try {
                    ISubscriberService processService = (ISubscriberService)Activator.GetObject(
                        typeof(ISubscriberService),
                        processURL);
                    processService.ConnectToPuppetMaster(serviceURL);
                    subscriberTable.Add(processName, processService);
                }
                catch (Exception e) {
                    throw e;
                }
                stateList[processName] = ProcessState.UNFROZEN;
            }
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
                if (!subscriberTable.TryGetValue(processName, out remoteService)) {
                    throw new InvalidProcessServiceException(processName);
                }
                try {
                    remoteService.ForceSubscribe(topicName);
                }
                catch (Exception e) {
                    throw e;
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
                if (!subscriberTable.TryGetValue(processName, out remoteService)) {
                    throw new InvalidProcessServiceException(processName);
                }
                try {
                    remoteService.ForceUnsubscribe(topicName);
                }
                catch (Exception e) {
                    throw e;
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
                if (!publisherTable.TryGetValue(processName, out remoteService)) {
                    throw new InvalidProcessServiceException(processName);
                }
                try {
                    for (int times = 0; times < publishTimes; times++) {
                        remoteService.ForcePublish(topicName, "Message");
                        Thread.Sleep(intervalTime);
                    }
                }
                catch (Exception e) {
                    throw e;
                }
            }
        }
        ///<summary>
        /// Gets Puppet Master Service status
        ///</summary>
        public void ExecuteStatusCommand() {
            String separator = "--------------------------------------------------------------------";
            String nl = Environment.NewLine;

            lock (stateList) {
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
                    try {
                        Console.WriteLine(process.GetStatus());
                    }
                    catch (Exception e) { }
                }
                Console.WriteLine(separator + nl +
                    " Publishers :" + nl);
                foreach (IPublisherService process in publisherTable.Values) {
                    try {
                        Console.WriteLine(process.GetStatus());
                    }
                    catch (Exception e) { }
                }
                Console.WriteLine(separator + nl +
                    " Subscribers :" + nl);
                foreach (ISubscriberService process in subscriberTable.Values) {
                    try {
                        Console.WriteLine(process.GetStatus());
                    }
                    catch (Exception e) { }
                }
            }
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
        private void TryRemoveService(String processName) {
            if (brokerTable.ContainsKey(processName)) {
                brokerTable.Remove(processName);
            }
            else if (publisherTable.ContainsKey(processName)) {
                publisherTable.Remove(processName);
            }
            else if (subscriberTable.ContainsKey(processName)) {
                subscriberTable.Remove(processName);
            }
        }
        ///<summary>
        /// Crashes a process
        ///</summary>
        public void ExecuteCrashCommand(String processName) {
            lock (stateList) {
                WriteIntoLog("Crash " + processName);
                if (!stateList[processName].Equals(ProcessState.CRASHED)) {
                    IGenericProcessService remoteService;
                    if (!TryGetService(processName, out remoteService)) {
                        throw new InvalidProcessServiceException(processName);
                    }
                    try {
                        remoteService.ForceCrash();
                    }
                    catch (Exception e) { }
                    TryRemoveService(processName);
                    stateList[processName] = ProcessState.CRASHED;
                }
            }
        }
        ///<summary>
        /// Freezes a process
        ///</summary>
        public void ExecuteFreezeCommand(String processName) {
            lock (stateList) {
                WriteIntoLog("Freeze " + processName);
                if (stateList[processName].Equals(ProcessState.UNFROZEN)) {
                    IGenericProcessService remoteService;
                    if (!TryGetService(processName, out remoteService)) {
                        throw new InvalidProcessServiceException(processName);
                    }
                    try {
                        remoteService.ForceFreeze();
                    }
                    catch (Exception e) {
                        throw e;
                    }
                    stateList[processName] = ProcessState.FROZEN;
                }
            }
        }
        ///<summary>
        /// Unfreezes a process
        ///</summary>
        public void ExecuteUnfreezeCommand(String processName) {
            lock (stateList) {
                WriteIntoLog("Unfreeze " + processName);
                if (stateList[processName].Equals(ProcessState.FROZEN)) {
                    IGenericProcessService remoteService;
                    if (!TryGetService(processName, out remoteService)) {
                        throw new InvalidProcessServiceException(processName);
                    }
                    try {
                        remoteService.ForceUnfreeze();
                    }
                    catch (Exception e) {
                        throw e;
                    }
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
            string log = "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + logMessage;
            lock (stateList) {
                using (StreamWriter w = File.AppendText("log.txt")) {
                    w.WriteLine("{0}", log);
                }

                using (StreamReader r = File.OpenText("log.txt")){
                    Console.WriteLine(log);
                }
            }
        }

        public void CloseProcesses() {
            lock (stateList) {
                foreach (ISubscriberService service in subscriberTable.Values) {
                    try {
                        service.ForceCrash();
                    }
                    catch (Exception) { }
                }
                foreach (IPublisherService service in publisherTable.Values) {
                    try {
                        service.ForceCrash();
                    }
                    catch (Exception) { }
                }
                foreach (IMessageBrokerService service in brokerTable.Values) {
                    try {
                        service.ForceCrash();
                    }
                    catch (Exception) { }
                }
                routingPolicy = RoutingPolicyType.FLOOD;
                ordering = OrderingType.FIFO;
                loggingLevel = LoggingLevelType.LIGHT;
                brokerTable = new Dictionary<String, IMessageBrokerService>();
                publisherTable = new Dictionary<String, IPublisherService>();
                subscriberTable = new Dictionary<String, ISubscriberService>();
                stateList = new Dictionary<String, ProcessState>();
            }
        }
    }
}
