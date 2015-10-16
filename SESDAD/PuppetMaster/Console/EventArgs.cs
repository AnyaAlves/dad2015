using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SESDAD.PuppetMaster {
    //<summary>
    // Type of Routing Policy
    //</summary>
    public enum RoutingPolicyType {
        flooding,
        filter
    };

    //<summary>
    // Type of Ordering
    //</summary>
    public enum OrderingType {
        NO,
        FIFO,
        TOTAL
    };

    //<summary>
    // Type of Logging Level
    //</summary>
    public enum LoggingLevelType {
        full,
        light
    };

    public class SiteEventArgs {
        public readonly String siteName, parentName;
        public SiteEventArgs(
            String siteNameValue,
            String parentNameValue) {
            siteName = siteNameValue;
            parentName = parentNameValue;
        }
    }
    public class CreateProcessEventArgs {
        public readonly String processName, siteName, urlName;
        public CreateProcessEventArgs(
            String processNameValue,
            String siteNameValue,
            String urlNameValue) {
            processName = processNameValue;
            siteName = siteNameValue;
            urlName = urlNameValue;
        }
    }

    public class RoutingPolicyEventArgs {
        public readonly RoutingPolicyType routingPolicy;
        public RoutingPolicyEventArgs(
            RoutingPolicyType routingPolicyValue) {
            routingPolicy = routingPolicyValue;
        }
    }

    public class OrderingEventArgs {
        public readonly OrderingType ordering;
        public OrderingEventArgs(
            OrderingType orderingValue) {
            ordering = orderingValue;
        }
    }

    public class SubscribeEventArgs {
        public readonly String subscriberName, topicName;
        public SubscribeEventArgs(
            String subscriberNameValue,
            String topicNameValue) {
            subscriberName = subscriberNameValue;
            topicName = topicNameValue;
        }
    }
    public class PublishEventArgs {
        public readonly String publisherName, topicName;
        public readonly int publishTimes, intervalTimes;
        public PublishEventArgs(
            String publisherNameValue,
            int publishTimesValue,
            String topicNameValue,
            int intervalTimesValue) {
            publisherName = publisherNameValue;
            publishTimes = publishTimesValue;
            topicName = topicNameValue;
            intervalTimes = intervalTimesValue;
        }
    }
    public class ControlProcessEventArgs {
        public readonly String processName;
        public ControlProcessEventArgs(
            String processNameValue) {
            processName = processNameValue;

        }
    }
    public class WaitEventArgs {
        public readonly int integerTime;
        public WaitEventArgs(
            int integerTimeValue) {
            integerTime = integerTimeValue;
        }
    }

    public class LoggingLevelEventArgs {
        public readonly LoggingLevelType loggingLevel;
        public LoggingLevelEventArgs(
            LoggingLevelType loggingLevelValue) {
            loggingLevel = loggingLevelValue;
        }
    }
}
