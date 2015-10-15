using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;

using SESDAD.PuppetMaster.CommonTypes;

namespace SESDAD.PuppetMaster.Library {
    public class PuppetMasterRemoteObject : MarshalByRefObject, IPuppetMaster {
        private EventHandler<SiteEventArgs> SiteHandler;
        private EventHandler<CreateProcessEventArgs> BrokerHandler;
        private EventHandler<CreateProcessEventArgs> PublisherHandler;
        private EventHandler<CreateProcessEventArgs> SubscriberHandler;
        private EventHandler<RoutingPolicyEventArgs> RoutingPolicyHandler;
        private EventHandler<OrderingEventArgs> OrderingHandler;
        private EventHandler<SubscribeEventArgs> SubscribeHandler;
        private EventHandler<SubscribeEventArgs> UnsubscribeHandler;
        private EventHandler<PublishEventArgs> PublishHandler;
        private EventHandler StatusHandler;
        private EventHandler<ControlProcessEventArgs> CrashHandler;
        private EventHandler<ControlProcessEventArgs> FreezeHandler;
        private EventHandler<ControlProcessEventArgs> UnfreezeHandler;
        private EventHandler<WaitEventArgs> WaitHandler;
        private EventHandler<LoggingLevelEventArgs> LoggingLevelHandler;

        public PuppetMasterRemoteObject(
            EventHandler<SiteEventArgs> SiteHandlerValue,
            EventHandler<CreateProcessEventArgs> BrokerHandlerValue,
            EventHandler<CreateProcessEventArgs> PublisherHandlerValue,
            EventHandler<CreateProcessEventArgs> SubscriberHandlerValue,
            EventHandler<RoutingPolicyEventArgs> RoutingPolicyHandlerValue,
            EventHandler<OrderingEventArgs> OrderingHandlerValue,
            EventHandler<SubscribeEventArgs> SubscribeHandlerValue,
            EventHandler<SubscribeEventArgs> UnsubscribeHandlerValue,
            EventHandler<PublishEventArgs> PublishHandlerValue,
            EventHandler StatusHandlerValue,
            EventHandler<ControlProcessEventArgs> CrashHandlerValue,
            EventHandler<ControlProcessEventArgs> FreezeHandlerValue,
            EventHandler<ControlProcessEventArgs> UnfreezeHandlerValue,
            EventHandler<WaitEventArgs> WaitHandlerValue,
            EventHandler<LoggingLevelEventArgs> LoggingLevelHandlerValue) {
            SiteHandler = SiteHandlerValue;
            BrokerHandler = BrokerHandlerValue;
            PublisherHandler = PublisherHandlerValue;
            SubscriberHandler = SubscriberHandlerValue;
            RoutingPolicyHandler = RoutingPolicyHandlerValue;
            OrderingHandler = OrderingHandlerValue;
            SubscribeHandler = SubscribeHandlerValue;
            UnsubscribeHandler = UnsubscribeHandlerValue;
            PublishHandler = PublishHandlerValue;
            StatusHandler = StatusHandlerValue;
            CrashHandler = CrashHandlerValue;
            FreezeHandler = FreezeHandlerValue;
            UnfreezeHandler = UnfreezeHandlerValue;
            WaitHandler = WaitHandlerValue;
            LoggingLevelHandler = LoggingLevelHandlerValue;
        }

        public void ExecuteSiteCommand(
            String siteName,
            String parentName) {
            SiteEventArgs eventArgs = new SiteEventArgs(siteName, parentName);
            try {
                SiteHandler(this, eventArgs);
            }
            catch (Exception) {
                throw new RemotingException();
            }
        }
        public void ExecuteBrokerCommand(
            String brokerName,
            String siteName,
            String urlName) {
            CreateProcessEventArgs eventArgs = new CreateProcessEventArgs(brokerName, siteName, urlName);
            try {
                BrokerHandler(this, eventArgs);
            }
            catch (Exception) {
                throw new RemotingException();
            }
        }
        public void ExecutePublisherCommand(
            String publisherName,
            String siteName,
            String urlName) {
            CreateProcessEventArgs eventArgs = new CreateProcessEventArgs(publisherName, siteName, urlName);
            try {
                PublisherHandler(this, eventArgs);
            }
            catch (Exception) {
                throw new RemotingException();
            }
        }
        public void ExecuteSubscriberCommand(
            String subscriberName,
            String siteName,
            String urlName) {
            CreateProcessEventArgs eventArgs = new CreateProcessEventArgs(subscriberName, siteName, urlName);
            try {
                SubscriberHandler(this, eventArgs);
            }
            catch (Exception) {
                throw new RemotingException();
            }
        }
        public void ExecuteFloodingRoutingPolicyCommand() {
            RoutingPolicyEventArgs eventArgs = new RoutingPolicyEventArgs(RoutingPolicyType.flooding);
            try {
                RoutingPolicyHandler(this, eventArgs);
            }
            catch (Exception) {
                throw new RemotingException();
            }
        }
        public void ExecuteFilterRoutingPolicyCommand() {
            RoutingPolicyEventArgs eventArgs = new RoutingPolicyEventArgs(RoutingPolicyType.filter);
            try {
                RoutingPolicyHandler(this, eventArgs);
            }
            catch (Exception) {
                throw new RemotingException();
            }
        }
        public void ExecuteNoOrderingCommand() {
            OrderingEventArgs eventArgs = new OrderingEventArgs(OrderingType.NO);
            try {
                OrderingHandler(this, eventArgs);
            }
            catch (Exception) {
                throw new RemotingException();
            }
        }
        public void ExecuteFIFOOrderingCommand() {
            OrderingEventArgs eventArgs = new OrderingEventArgs(OrderingType.FIFO);
            try {
                OrderingHandler(this, eventArgs);
            }
            catch (Exception) {
                throw new RemotingException();
            }
        }
        public void ExecuteTotalOrderingCommand() {
            OrderingEventArgs eventArgs = new OrderingEventArgs(OrderingType.FIFO);
            try {
                OrderingHandler(this, eventArgs);
            }
            catch (Exception) {
                throw new RemotingException();
            }
        }
        public void ExecuteSubscribeCommand(
            String subscriberName,
            String topicName) {
            SubscribeEventArgs eventArgs = new SubscribeEventArgs(subscriberName, topicName);
            try {
                SubscribeHandler(this, eventArgs);
            }
            catch (Exception) {
                throw new RemotingException();
            }
        }
        public void ExecuteUnsubscribeCommand(
            String subscriberName,
            String topicName) {
            SubscribeEventArgs eventArgs = new SubscribeEventArgs(subscriberName, topicName);
            try {
                UnsubscribeHandler(this, eventArgs);
            }
            catch (Exception) {
                throw new RemotingException();
            }
        }
        public void ExecutePublishCommand(
            String publisherName,
            int publishTimes,
            String topicName,
            int intervalTime) {
            PublishEventArgs eventArgs = new PublishEventArgs(publisherName, publishTimes, topicName, intervalTime);
            try {
                PublishHandler(this, eventArgs);
            }
            catch (Exception) {
                throw new RemotingException();
            }
        }
        public void ExecuteStatusCommand() {
            try {
                StatusHandler(this, null);
            }
            catch (Exception) {
                throw new RemotingException();
            }
        }
        public void ExecuteCrashCommand(String processName) {
            ControlProcessEventArgs eventArgs = new ControlProcessEventArgs(processName);
            try {
                CrashHandler(this, eventArgs);
            }
            catch (Exception) {
                throw new RemotingException();
            }
        }
        public void ExecuteFreezeCommand(String processName) {
            ControlProcessEventArgs eventArgs = new ControlProcessEventArgs(processName);
            try {
                FreezeHandler(this, eventArgs);
            }
            catch (Exception) {
                throw new RemotingException();
            }
        }
        public void ExecuteUnfreezeCommand(String processName) {
            ControlProcessEventArgs eventArgs = new ControlProcessEventArgs(processName);
            try {
                UnfreezeHandler(this, eventArgs);
            }
            catch (Exception) {
                throw new RemotingException();
            }
        }
        public void ExecuteWaitCommand(int waitingTime) {
            WaitEventArgs eventArgs = new WaitEventArgs(waitingTime);
            try {
                WaitHandler(this, eventArgs);
            }
            catch (Exception) {
                throw new RemotingException();
            }
        }
        public void ExecuteFullLoggingLevelCommand() {
            LoggingLevelEventArgs eventArgs = new LoggingLevelEventArgs(LoggingLevelType.full);
            try {
                LoggingLevelHandler(this, eventArgs);
            }
            catch (Exception) {
                throw new RemotingException();
            }
        }
        public void ExecuteLightLoggingLevelCommand() {
            LoggingLevelEventArgs eventArgs = new LoggingLevelEventArgs(LoggingLevelType.light);
            try {
                LoggingLevelHandler(this, eventArgs);
            }
            catch (Exception) {
                throw new RemotingException();
            }
        }
    }
}
