using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Text.RegularExpressions;

using SESDAD.CommonTypes;

namespace SESDAD.PuppetMaster {
    internal enum ProcessType {
        BROKER,
        PUBLISHER,
        SUBSCRIBER
    }
    ///<summary>
    /// Puppet Master Service
    ///</summary>
    public class PuppetMasterService : MarshalByRefObject, IPuppetMasterService, IAdministratorService {
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
        private IDictionary<String, bool> activeProcessList;
        private IDictionary<String, String> siteNameToBrokerURITable;
        private IDictionary<String, Tuple<String, ProcessType>> waitingTable;
        private IDictionary<String, IBrokerRemoteService> brokerToBrokerRemoteObjectTable;
        private IDictionary<String, IPublisherRemoteObject> publisherToPublisherRemoteObjectTable;
        private IDictionary<String, ISubscriberRemoteObject> subscriberToSubscriberRemoteObjectTable;
        ///<summary>
        /// Puppet Master Service constructor
        ///</summary>
        public PuppetMasterService() {
            log = "";
            routingPolicy = RoutingPolicyType.FLOODING;
            ordering = OrderingType.FIFO;
            loggingLevel = LoggingLevelType.LIGHT;
            siteNameToBrokerURITable = new Dictionary<String, String>();
            brokerToBrokerRemoteObjectTable = new Dictionary<String, IBrokerRemoteService>();
            publisherToPublisherRemoteObjectTable = new Dictionary<String, IPublisherRemoteObject>();
            subscriberToSubscriberRemoteObjectTable = new Dictionary<String, ISubscriberRemoteObject>();
            waitingTable = new Dictionary<String, Tuple<String, ProcessType>>();
            activeProcessList = new Dictionary<String, bool>();
            BROKERFILE = "SESDAD.MessageBroker.exe";
            PUBLISHERFILE = "SESDAD.Publisher.exe";
            SUBSCRIBERFILE = "SESDAD.Subscriber.exe";
        }
        ///<summary>
        /// Puppet Master Service routing policy
        ///</summary>
        public RoutingPolicyType RoutingPolicy {
            set {
                routingPolicy = value;
            }
        }
        ///<summary>
        /// Puppet Master Service ordering
        ///</summary>
        public OrderingType Ordering {
            set {
                ordering = value;
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
            String brokerName,
            String siteName,
            String brokerURI,
            String parentBrokerURI) {
            System.Diagnostics.Process.Start(BROKERFILE, brokerName + " " + siteName + " " + brokerURI + " " + parentBrokerURI);
            siteNameToBrokerURITable.Add(siteName, brokerURI);
            waitingTable.Add(brokerName, new Tuple<String, ProcessType>(brokerURI, ProcessType.BROKER));
        }
        ///<summary>
        /// Creates a publisher process
        ///</summary>
        public void ExecutePublisherCommand(
            String publisherName,
            String siteName,
            String publisherURL) {
            String brokerURI;
            if (!siteNameToBrokerURITable.TryGetValue(siteName, out brokerURI)) {
                return;
            }
            System.Diagnostics.Process.Start(PUBLISHERFILE, publisherName + " " + siteName + " " + publisherURL + " " + brokerURI);
            waitingTable.Add(publisherName, new Tuple<String, ProcessType>(publisherURL, ProcessType.PUBLISHER));
        }
        ///<summary>
        /// Creates a subscriber process
        ///</summary>
        public void ExecuteSubscriberCommand(
            String subscriberName,
            String siteName,
            String subscriberURL) {
            String brokerURI;
            if (!siteNameToBrokerURITable.TryGetValue(siteName, out brokerURI)) {
                return;
            }
            System.Diagnostics.Process.Start(SUBSCRIBERFILE, subscriberName + " " + siteName + " " + subscriberURL + " " + brokerURI);
            waitingTable.Add(subscriberName, new Tuple<String, ProcessType>(subscriberURL, ProcessType.SUBSCRIBER));
        }
        ///<summary>
        /// Subscribes into a topic
        ///</summary>
        public void ExecuteSubscribeCommand(
            String subscriberName,
            String topicName) {
            log += String.Format(
                "[{0}] Subscribe {1} Subscribe {2}",
                DateTime.Now.ToString(),
                subscriberName,
                topicName);
        }
        ///<summary>
        /// Unsubscribes from a topic
        ///</summary>
        public void ExecuteUnsubscribeCommand(
            String subscriberName,
            String topicName) {
            if (loggingLevel == LoggingLevelType.full) {
                log += String.Format(
                    "[{0}] Subscribe {1} Unsubscribe {2}",
                    DateTime.Now.ToString(),
                    subscriberName,
                    topicName);
            }
        }
        ///<summary>
        /// Publishes a message
        ///</summary>
        public void ExecutePublishCommand(
            String publisherName,
            int publishTimes,
            String topicName,
            int intervalTime) {
            log += String.Format(
                "[{0}] Publish {1} Publish {2} Ontopic {3} Interval  {4}",
                DateTime.Now.ToString(),
                publisherName,
                publishTimes,
                topicName,
                intervalTime);
        }
        ///<summary>
        /// Gets Puppet Master Service status
        ///</summary>
        public void ExecuteStatusCommand() {
            String acc = "";
            acc += " Sites:" + Environment.NewLine;
            foreach (String siteName in siteNameToBrokerURITable.Keys.ToList()) {
                acc += " -> " + siteName + Environment.NewLine;
            }
            acc += " Brokers:" + Environment.NewLine;
            foreach (String processName in brokerToBrokerRemoteObjectTable.Keys.ToList()) {
                acc += " -> " + processName + Environment.NewLine;
            }
            acc += " Publishers:" + Environment.NewLine;
            foreach (String processName in publisherToPublisherRemoteObjectTable.Keys.ToList()) {
                acc += " -> " + processName + Environment.NewLine;
            }
            acc += " Subscribers:" + Environment.NewLine;
            foreach (String processName in subscriberToSubscriberRemoteObjectTable.Keys.ToList()) {
                acc += " -> " + processName + Environment.NewLine;
            }
            acc += " Loading:" + Environment.NewLine;
            //FIXME: ONLY BROKERS ARE DISPLAYED
            foreach (String processName in waitingTable.Keys.ToList()) {
                acc += " -> " + processName + Environment.NewLine;
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
        ///<summary>
        /// Crashes a process
        ///</summary>
        public void ExecuteCrashCommand(String processName) {
            if (activeProcessList.ContainsKey(processName)) {
                IProcessRemoteService remoteService;
                if (brokerToBrokerRemoteObjectTable.ContainsKey(processName)) {
                    remoteService = brokerToBrokerRemoteObjectTable[processName];
                }
                else if (publisherToPublisherRemoteObjectTable.ContainsKey(processName)) {
                    remoteService = publisherToPublisherRemoteObjectTable[processName];
                }
                else if (subscriberToSubscriberRemoteObjectTable.ContainsKey(processName)) {
                    remoteService = subscriberToSubscriberRemoteObjectTable[processName];
                }
                //Crash process
            }
        }
        ///<summary>
        /// Freezes a process
        ///</summary>
        public void ExecuteFreezeCommand(String processName) {
            //Choose a process
            bool isFrozen;
            if (activeProcessList.TryGetValue(processName, out isFrozen) && !isFrozen) {
                IProcessRemoteService remoteService;
                if (brokerToBrokerRemoteObjectTable.ContainsKey(processName)) {
                    remoteService = brokerToBrokerRemoteObjectTable[processName];
                }
                else if (publisherToPublisherRemoteObjectTable.ContainsKey(processName)) {
                    remoteService = publisherToPublisherRemoteObjectTable[processName];
                }
                else if (subscriberToSubscriberRemoteObjectTable.ContainsKey(processName)) {
                    remoteService = subscriberToSubscriberRemoteObjectTable[processName];
                }
                //Freeze process if Unfreeze
                activeProcessList[processName] = true;
            }
        }
        ///<summary>
        /// Unfreezes a process
        ///</summary>
        public void ExecuteUnfreezeCommand(String processName) {
            //Choose a process
            bool isFrozen;
            if (activeProcessList.TryGetValue(processName, out isFrozen) && isFrozen) {
                IProcessRemoteService remoteService;
                if (brokerToBrokerRemoteObjectTable.ContainsKey(processName)) {
                    remoteService = brokerToBrokerRemoteObjectTable[processName];
                }
                else if (publisherToPublisherRemoteObjectTable.ContainsKey(processName)) {
                    remoteService = publisherToPublisherRemoteObjectTable[processName];
                }
                else if (subscriberToSubscriberRemoteObjectTable.ContainsKey(processName)) {
                    remoteService = subscriberToSubscriberRemoteObjectTable[processName];
                }
                //Unfreeze process if Unfreeze
                activeProcessList[processName] = false;
            }
        }
        ///<summary>
        /// Resumes a connection establishment
        ///</summary>
        public void ConfirmConnection(String processName) {
            if (waitingTable.ContainsKey(processName)) {
                switch (waitingTable[processName].Item2) {
                    case ProcessType.BROKER:
                        brokerToBrokerRemoteObjectTable.Add(processName,  (IBrokerRemoteService)Activator.GetObject(
                            typeof(IBrokerRemoteService),
                            waitingTable[processName].Item1));
                        break;
                    case ProcessType.PUBLISHER:
                        publisherToPublisherRemoteObjectTable.Add(processName, (IPublisherRemoteObject)Activator.GetObject(
                            typeof(IPublisherRemoteObject),
                            waitingTable[processName].Item1));
                        break;
                    case ProcessType.SUBSCRIBER:
                        subscriberToSubscriberRemoteObjectTable.Add(processName,  (ISubscriberRemoteObject)Activator.GetObject(
                            typeof(ISubscriberRemoteObject),
                            waitingTable[processName].Item1));
                        break;
                }
                waitingTable.Remove(processName);
                activeProcessList.Add(processName, false);
            }
        }
        ///<summary>
        /// Writes into the Puppet Master Service Log
        ///</summary>
        public void WriteIntoLog(String logMessage) {
        
        }
    }
}
