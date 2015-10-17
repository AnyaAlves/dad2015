using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;

using SESDAD.PuppetMaster;

namespace SESDAD.PuppetMaster {
    public class PuppetMasterService : MarshalByRefObject, IPuppetMasterService {
        private String log;
        private RoutingPolicyType routingPolicy;
        private OrderingType ordering;
        private LoggingLevelType loggingLevel;

        public PuppetMasterService() {
            log = "";
            routingPolicy = RoutingPolicyType.flooding;
            ordering = OrderingType.FIFO;
            loggingLevel = LoggingLevelType.light;
        }

        public void SetPolicies(
            RoutingPolicyType newRoutingPolicy,
            OrderingType newOrdering,
            LoggingLevelType newLoggingLevel) {
            routingPolicy = newRoutingPolicy;
            ordering = newOrdering;
            loggingLevel = newLoggingLevel;
        }

        public void ExecuteSiteCommand(
            String siteName,
            String parentName) {
        }
        public void ExecuteBrokerCommand(
            String brokerName,
            String siteName,
            String urlName) {
        }
        public void ExecutePublisherCommand(
            String publisherName,
            String siteName,
            String urlName) {
        }
        public void ExecuteSubscriberCommand(
            String subscriberName,
            String siteName,
            String urlName) {
        }
        public void ExecuteFloodingRoutingPolicyCommand() {
            routingPolicy = RoutingPolicyType.flooding;
        }
        public void ExecuteFilterRoutingPolicyCommand() {
            routingPolicy = RoutingPolicyType.filter;
        }
        public void ExecuteNoOrderingCommand() {
            ordering = OrderingType.NO;
        }
        public void ExecuteFIFOOrderingCommand() {
            ordering = OrderingType.FIFO;
        }
        public void ExecuteTotalOrderingCommand() {
            ordering = OrderingType.TOTAL;
        }
        public void ExecuteSubscribeCommand(
            String subscriberName,
            String topicName) {
            log += String.Format(
                "[{0}] Subscribe {1} Subscribe {2}",
                DateTime.Now.ToString(),
                subscriberName,
                topicName);
        }
        public void ExecuteUnsubscribeCommand(
            String subscriberName,
            String topicName) {
            log += String.Format(
                "[{0}] Subscribe {1} Unsubscribe {2}",
                DateTime.Now.ToString(),
                subscriberName,
                topicName);
        }
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
        public void ExecuteCrashCommand(String processName) {
        }
        public void ExecuteFreezeCommand(String processName) {
        }
        public void ExecuteUnfreezeCommand(String processName) {
        }
        public void ExecuteWaitCommand(int waitingTime) {
        }
        public void ExecuteFullLoggingLevelCommand() {
            loggingLevel = LoggingLevelType.full;
        }
        public void ExecuteLightLoggingLevelCommand() {
            loggingLevel = LoggingLevelType.light;
        }
    }
}
