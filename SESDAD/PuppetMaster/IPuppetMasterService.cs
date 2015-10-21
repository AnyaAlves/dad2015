using System;
using SESDAD.CommonTypes;

namespace SESDAD.PuppetMaster {
    ///<summary>
    /// Puppet Master Service interface
    ///</summary>
    internal interface IPuppetMasterService {
        ///<summary>
        /// Puppet Master Service interface routing policy
        ///</summary>
        RoutingPolicyType RoutingPolicy { set; }
        ///<summary>
        /// Puppet Master Service interface ordering
        ///</summary>
        OrderingType Ordering { set; }
        ///<summary>
        /// Puppet Master Service interface logging level
        ///</summary>
        LoggingLevelType LoggingLevel { set; }
        ///<summary>
        /// Creates a broker process
        ///</summary>
        void ExecuteBrokerCommand(String brokerName, String SiteName, String urlName, String parentBrokerURI);
        ///<summary>
        /// Creates a publisher process
        ///</summary>
        void ExecutePublisherCommand(String publisherName, String SiteName, String urlName, String brokerURL);
        ///<summary>
        /// Creates a subscriber process
        ///</summary>
        void ExecuteSubscriberCommand(String subscriberName, String SiteName, String urlName, String brokerURL);
        ///<summary>
        /// Subscribes into a topic
        ///</summary>
        void ExecuteSubscribeCommand(String subscriberName, String topicName);
        ///<summary>
        /// Unsubscribes from a topic
        ///</summary>
        void ExecuteUnsubscribeCommand(String subscriberName, String topicName);
        ///<summary>
        /// Publishes a message
        ///</summary>
        void ExecutePublishCommand(String publisherName, int publishTimes, String topicName, int intervalTime);
        ///<summary>
        /// Gets Puppet Master Service interface status
        ///</summary>
        void ExecuteStatusCommand();
        ///<summary>
        /// Crashes a process
        ///</summary>
        void ExecuteCrashCommand(String processName);
        ///<summary>
        /// Freezes a process
        ///</summary>
        void ExecuteFreezeCommand(String processName);
        ///<summary>
        /// Unfreezes a process
        ///</summary>
        void ExecuteUnfreezeCommand(String processName);
    }
}
