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
    public class PuppetMasterService : MarshalByRefObject, IPuppetMasterService/*, IAdministrationService*/ {
        private String log;
        private RoutingPolicyType routingPolicy;
        private OrderingType ordering;
        private LoggingLevelType loggingLevel;
        private IDictionary<String, ISubscriberRemoteObject> subscriberToSubscriberRemoteObjectTable;
        ///private IDictionary<String, IPublisherRemoteObject> publisherToPublisherRemoteObjectTable;
        private IDictionary<String, IBrokerRemoteService> brokerToBrokerRemoteObjectTable;
        private IDictionary<String, String> siteNameToBrokerURITable;
        private IDictionary<String, Tuple<String, ProcessType>> waitingTable;
        private readonly String REGEXURL,
                                BROKERFILE,
                                PUBLISHERFILE,
                                SUBSCRIBERFILE;
        private IDictionary<String, bool> frozenList;
        ///<summary>
        /// Puppet Master Service constructor
        ///</summary>
        public PuppetMasterService() {
            log = "";
            routingPolicy = RoutingPolicyType.FLOODING;
            ordering = OrderingType.FIFO;
            loggingLevel = LoggingLevelType.LIGHT;
            siteNameToBrokerURITable = new Dictionary<String, String>();
            waitingTable = new Dictionary<String, Tuple<String, ProcessType>>();
            frozenList = new Dictionary<String, bool>();
            REGEXURL = @"^tcp:///([\w\.])+:\d{1,5}/\w+$";
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
            System.Console.WriteLine(
                "------------------------------------------------------------------------------" + Environment.NewLine +
                " Routing Policy : " + routingPolicy.ToString() + Environment.NewLine +
                " Ordering :       " + ordering.ToString() + Environment.NewLine +
                " Logging Level :  " + loggingLevel.ToString() + Environment.NewLine +
                " Log :            " + log + Environment.NewLine +
                "------------------------------------------------------------------------------");
        }
        ///<summary>
        /// Crashes a process
        ///</summary>
        public void ExecuteCrashCommand(String processName) {
            ///Choose a process
            ///Crash process
        }
        ///<summary>
        /// Freezes a process
        ///</summary>
        public void ExecuteFreezeCommand(String processName) {
            ///Choose a process
            bool isFrozen;
            if (frozenList.TryGetValue(processName, out isFrozen) && !isFrozen) {
                //Freeze process if Unfreeze
                //isFrozen[processName] = true;
            }
        }
        ///<summary>
        /// Unfreezes a process
        ///</summary>
        public void ExecuteUnfreezeCommand(String processName) {
            //Choose a process
            bool isFrozen;
            if (frozenList.TryGetValue(processName, out isFrozen) && isFrozen) {
                //Unfreeze process if Unfreeze
                frozenList[processName] = false;
            }
        }
        ///<summary>
        /// Waits for a determined time
        ///</summary>
        public void ExecuteWaitCommand(int waitingTime) {
        }
        ///<summary>
        /// Resumes a connection establishment
        ///</summary>
        public void Continue(String processName) {
            if (waitingTable.ContainsKey(processName)) {
                switch (waitingTable[processName].Item2) {
                    case ProcessType.BROKER:
                        IBrokerRemoteService remoteService = (IBrokerRemoteService)Activator.GetObject(
                            typeof(IBrokerRemoteService),
                            waitingTable[processName].Item1);
                        brokerToBrokerRemoteObjectTable.Add(processName, remoteService);
                        break;
                    case ProcessType.PUBLISHER:
                        //IPublisherRemoteObject remoteObject = (IPublisherRemoteObject)Activator.GetObject(
                        //    typeof(IBrokerPublisherObject),
                        //    waitingTable[processName].Item1);
                        //brokerToBrokerRemoteObjectTable.Add(processName, remoteObject);
                        break;
                    case ProcessType.SUBSCRIBER:
                        ISubscriberRemoteObject remoteObject = (ISubscriberRemoteObject)Activator.GetObject(
                            typeof(ISubscriberRemoteObject),
                            waitingTable[processName].Item1);
                        subscriberToSubscriberRemoteObjectTable.Add(processName, remoteObject);
                        break;
                    default: break;
                }
                waitingTable.Remove(processName);
                frozenList.Add(processName, false);
            }
        }
        ///<summary>
        /// Writes into the Puppet Master Service Log
        ///</summary>
        public void WriteIntoLog(String logMessage) {
        
        }
    }
}
