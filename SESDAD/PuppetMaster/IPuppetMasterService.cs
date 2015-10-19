using System;

namespace SESDAD.PuppetMaster {

    //<summary>
    // Type of Logging Level
    //</summary>
    public interface IPuppetMasterService {
        RoutingPolicyType RoutingPolicy { set; }
        OrderingType Ordering { set; }
        LoggingLevelType LoggingLevel { set; }
        String ParentBrokerURI { set; }
        void ExecuteBrokerCommand(String brokerName, String SiteName, String urlName, String parentBrokerURI);
        void ExecutePublisherCommand(String publisherName, String SiteName, String urlName);
        void ExecuteSubscriberCommand(String subscriberName, String SiteName, String urlName);
        void ExecuteSubscribeCommand(String subscriberName, String topicName);
        void ExecuteUnsubscribeCommand(String subscriberName, String topicName);
        void ExecutePublishCommand(String publisherName, int publishTimes, String topicName, int intervalTime);
        String ExecuteStatusCommand();
        void ExecuteCrashCommand(String processName);
        void ExecuteFreezeCommand(String processName);
        void ExecuteUnfreezeCommand(String processName);
        void ExecuteWaitCommand(int waitingTime);
    }
}
