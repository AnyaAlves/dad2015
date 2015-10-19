using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Text.RegularExpressions;

using SESDAD.CommonTypes;

namespace SESDAD.PuppetMaster {
    ///<summary>
    /// Puppet Master Service
    ///</summary>
    public class PuppetMasterService : MarshalByRefObject, IPuppetMasterService {
        private String log;
        private RoutingPolicyType routingPolicy;
        private OrderingType ordering;
        private LoggingLevelType loggingLevel;
        private IDictionary<String, ISubscriberRemoteObject> subscriberToSubscriberRemoteObjectTable;
        ///private IDictionary<String, IPublisherRemoteObject> publisherToPublisherRemoteObjectTable;
        private IDictionary<String, IBrokerRemoteService> brokerToBrokerRemoteObjectTable;
        private IDictionary<String, String> siteNameToBrokerURITable;
        private readonly String REGEXURL,
                                BROKERFILE,
                                PUBLISHERFILE,
                                SUBSCRIBERFILE;
        private IDictionary<String, bool> isFrozen;
        ///<summary>
        /// Puppet Master Service constructor
        ///</summary>
        public PuppetMasterService() {
            log = "";
            routingPolicy = RoutingPolicyType.flooding;
            ordering = OrderingType.FIFO;
            loggingLevel = LoggingLevelType.light;
            siteNameToBrokerURITable = new Dictionary<String, String>();
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
            String urlName,
            String parentBrokerURI) {
            System.Diagnostics.Process.Start(BROKERFILE, brokerName + " " + siteName + " " + urlName + " " + parentBrokerURI);
            IBrokerRemoteService remoteService = (IBrokerRemoteService)Activator.GetObject(
                typeof(IBrokerRemoteService),
                urlName);
            brokerToBrokerRemoteObjectTable.Add(brokerName, remoteService);
            siteNameToBrokerURITable.Add(siteName, urlName);
            isFrozen.Add(brokerName, false);
        }
        ///<summary>
        /// Creates a publisher process
        ///</summary>
        public void ExecutePublisherCommand(
            String publisherName,
            String siteName,
            String urlName) {
            String brokerURI;
            if (siteNameToBrokerURITable.TryGetValue(siteName, out brokerURI)) {
                return;
            }
            System.Diagnostics.Process.Start(PUBLISHERFILE, publisherName + " " + siteName + " " + urlName + " " + brokerURI);
            //IPublisherRemoteObject remoteObject = (IPublisherRemoteObject)Activator.GetObject(
            //    typeof(IBrokerPublisherObject),
            //    urlName);
            //brokerToBrokerRemoteObjectTable.Add(publisherName, remoteObject);
            isFrozen.Add(publisherName, false);
        }
        ///<summary>
        /// Creates a subscriber process
        ///</summary>
        public void ExecuteSubscriberCommand(
            String subscriberName,
            String siteName,
            String urlName) {
            String brokerURI;
            if (siteNameToBrokerURITable.TryGetValue(siteName, out brokerURI)) {
                return;
            }
            System.Diagnostics.Process.Start(SUBSCRIBERFILE, subscriberName + " " + siteName + " " + urlName + " " + brokerURI);
            ISubscriberRemoteObject remoteObject = (ISubscriberRemoteObject)Activator.GetObject(
                typeof(ISubscriberRemoteObject),
                urlName);
            subscriberToSubscriberRemoteObjectTable.Add(subscriberName, remoteObject);
            isFrozen.Add(subscriberName, false);
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
        public String ExecuteStatusCommand() {
            String logMessage = "Log:";
            if (log.Equals("")) {
                logMessage += " EMPTY";
            }
            else {
                logMessage += "\n" + log;
            }
            return "------------------------------------------------------------------------------\nRouting Policy: " + routingPolicy.ToString() + "\nOrdering: " + ordering.ToString() + "\nLogging Level: " + loggingLevel.ToString() + "\n" + logMessage + "\n------------------------------------------------------------------------------";
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
            if (!isFrozen[processName]) {
                ///Freeze process if Unfreeze
            }
            isFrozen[processName] = true;
        }
        ///<summary>
        /// Unfreezes a process
        ///</summary>
        public void ExecuteUnfreezeCommand(String processName) {
            ///Choose a process
            if (isFrozen[processName]) {
                ///Unfreeze process if Unfreeze
            }
            isFrozen[processName] = false;
        }
        ///<summary>
        /// Waits for a determined time
        ///</summary>
        public void ExecuteWaitCommand(int waitingTime) {
        }
    }
}
