using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public class PuppetMasterService : MarshalByRefObject, IPuppetMasterService, IPuppetMasterRemoteService {
        // Log
        private String log;
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
        private IDictionary<String, IBrokerRemoteService> brokerTable;
        private IDictionary<String, IPublisherRemoteService> publisherTable;
        private IDictionary<String, ISubscriberRemoteService> subscriberTable;
        ///<summary>
        /// Puppet Master Service constructor
        ///</summary>
        public PuppetMasterService() {
            log = "";
            routingPolicy = RoutingPolicyType.FLOODING;
            ordering = OrderingType.FIFO;
            loggingLevel = LoggingLevelType.LIGHT;
            brokerTable = new Dictionary<String, IBrokerRemoteService>();
            publisherTable = new Dictionary<String, IPublisherRemoteService>();
            subscriberTable = new Dictionary<String, ISubscriberRemoteService>();
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
                foreach (IBrokerRemoteService brokerService in brokerTable.Values.ToList()) {
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
                foreach (IBrokerRemoteService brokerService in brokerTable.Values.ToList()) {
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
            String parentBrokerURI) {
            System.Diagnostics.Process.Start(BROKERFILE, processName + " " + siteName + " " + processURL + " " + parentBrokerURI);
            stateList.Add(processName, ProcessState.OFFLINE);
        }
        ///<summary>
        /// Creates a publisher process
        ///</summary>
        public void ExecutePublisherCommand(
            String processName,
            String siteName,
            String processURL,
            String brokerURL) {
            System.Diagnostics.Process.Start(PUBLISHERFILE, processName + " " + siteName + " " + processURL + " " + brokerURL);
            stateList.Add(processName, ProcessState.OFFLINE);
        }
        ///<summary>
        /// Creates a subscriber process
        ///</summary>
        public void ExecuteSubscriberCommand(
            String processName,
            String siteName,
            String processURL,
            String brokerURL) {
            System.Diagnostics.Process.Start(SUBSCRIBERFILE, processName + " " + siteName + " " + processURL + " " + brokerURL);
            stateList.Add(processName, ProcessState.OFFLINE);
        }
        ///<summary>
        /// Subscribes into a topic
        ///</summary>
        public void ExecuteSubscribeCommand(
            String processName,
            String topicName) {
            if (stateList[processName].Equals(ProcessState.FROZEN) ||
                stateList[processName].Equals(ProcessState.UNFROZEN)) {
                ISubscriberRemoteService remoteService;
                if (subscriberTable.TryGetValue(processName, out remoteService)) {
                    remoteService.Subscribe(topicName);
                }
            }
        }
        ///<summary>
        /// Unsubscribes from a topic
        ///</summary>
        public void ExecuteUnsubscribeCommand(
            String processName,
            String topicName) {
            if (stateList[processName].Equals(ProcessState.FROZEN) ||
                stateList[processName].Equals(ProcessState.UNFROZEN)) {
                ISubscriberRemoteService remoteService;
                if (subscriberTable.TryGetValue(processName, out remoteService)) {
                    remoteService.Unsubscribe(topicName);
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
            if (stateList[processName].Equals(ProcessState.FROZEN) ||
                stateList[processName].Equals(ProcessState.UNFROZEN)) {
                IPublisherRemoteService remoteService;
                if (publisherTable.TryGetValue(processName, out remoteService)) {
                    for (int times = 0; times < publishTimes; times++) {
                        remoteService.Publish(topicName, "batata");
                    }
                }
            }
        }
        ///<summary>
        /// Gets Puppet Master Service status
        ///</summary>
        public void ExecuteStatusCommand() {
            String acc = "";
            acc += " Processes:" + Environment.NewLine;
            foreach (KeyValuePair<String, ProcessState> process in stateList.ToList()) {
                acc += " -> " + process.Key + " is " + process.Value.ToString() + Environment.NewLine;
            }
            System.Console.WriteLine(
                "------------------------------------------------------------------------------" + Environment.NewLine +
                " Routing Policy : " + routingPolicy.ToString() + Environment.NewLine +
                " Ordering :       " + ordering.ToString() + Environment.NewLine +
                " Logging Level :  " + loggingLevel.ToString() + Environment.NewLine +
                " Log :            " + log + Environment.NewLine +
                acc +
                "------------------------------------------------------------------------------");
        }
        private bool TryGetService(String processName, out IProcessRemoteService remoteService) {
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
            if (stateList[processName].Equals(ProcessState.FROZEN) ||
                stateList[processName].Equals(ProcessState.UNFROZEN)) {
                IProcessRemoteService remoteService;
                if (TryGetService(processName, out remoteService)) {
                    stateList[processName] = ProcessState.UNDEFINED;
                    remoteService.Crash();
                    stateList[processName] = ProcessState.CRASHED;
                }
            }
        }
        ///<summary>
        /// Freezes a process
        ///</summary>
        public void ExecuteFreezeCommand(String processName) {
            if (stateList[processName].Equals(ProcessState.UNFROZEN)) {
                IProcessRemoteService remoteService;
                if (TryGetService(processName, out remoteService)) {
                    stateList[processName] = ProcessState.UNDEFINED;
                    remoteService.Freeze();
                    stateList[processName] = ProcessState.FROZEN;
                }
            }
        }
        ///<summary>
        /// Unfreezes a process
        ///</summary>
        public void ExecuteUnfreezeCommand(String processName) {
            if (stateList[processName].Equals(ProcessState.FROZEN)) {
                IProcessRemoteService remoteService;
                if (TryGetService(processName, out remoteService)) {
                    stateList[processName] = ProcessState.UNDEFINED;
                    remoteService.Unfreeze();
                    stateList[processName] = ProcessState.UNFROZEN;
                }
            }
        }
        ///<summary>
        /// Resumes a connection establishment with a broker
        ///</summary>
        public void ConfirmBrokerConnection(String processName, String processURL) {
            brokerTable.Add(processName, (IBrokerRemoteService)Activator.GetObject(
                typeof(IBrokerRemoteService),
                processURL));
            stateList[processName] = ProcessState.UNFROZEN;
        }
        ///<summary>
        /// Resumes a connection establishment with a publisher
        ///</summary>
        public void ConfirmPublisherConnection(String processName, String processURL) {
            publisherTable.Add(processName, (IPublisherRemoteService)Activator.GetObject(
                typeof(IPublisherRemoteService),
                processURL));
            stateList[processName] = ProcessState.UNFROZEN;
        }
        ///<summary>
        /// Resumes a connection establishment with a subscriber
        ///</summary>
        public void ConfirmSubscriberConnection(String processName, String processURL) {
            subscriberTable.Add(processName, (ISubscriberRemoteService)Activator.GetObject(
                typeof(ISubscriberRemoteService),
                processURL));
            stateList[processName] = ProcessState.UNFROZEN;
        }
        ///<summary>
        /// Writes into the Puppet Master Service Log while in full log
        ///</summary>
        public void WriteIntoFullLog(String logMessage) {
            if (loggingLevel == LoggingLevelType.FULL) {
                log += logMessage + Environment.NewLine;
            }
        }
        ///<summary>
        /// Writes into the Puppet Master Service Log
        ///</summary>
        public void WriteIntoLog(String logMessage) {
            log += logMessage + Environment.NewLine;
        }
    }
}
