using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SESDAD.CommonTypes {
    public interface IProcessRemoteService {       
        void Freeze();
        void Unfreeze();
        void Crash();

        void Ping();
    }

    public interface IBrokerRemoteService : IProcessRemoteService {
        ///<summary>
        /// Broker Remote Service Interface name
        ///</summary>
        String ProcessName { get; }
        ///<summary>
        /// Broker Remote Service interface routing policy
        ///</summary>
        RoutingPolicyType RoutingPolicy { set; }
        ///<summary>
        /// Broker Remote Service interface ordering
        ///</summary>
        OrderingType Ordering { set; }
        void Publish(String processName, String processURL, Entry entry);

        void Subscribe(String processName, String processURL, String topicName);
        void Unsubscribe(String processName, String processURL, String topicName);

        void RegisterBroker(String processName, String processURL);
        void RegisterPublisher(String processName, String processURL);
        void RegisterSubscriber(String processName, String processURL);
    }

    public interface ISubscriberRemoteService : IProcessRemoteService {
        void DeliverEntry(Entry entry);
        void Subscribe(String topicName);
        void Unsubscribe(String topicName);
    }

    public interface IPublisherRemoteService : IProcessRemoteService {
        void Publish(String topicName, String content);
    }

    public interface IPuppetMasterRemoteService {
        void RegisterBroker(String processName, String processURL);
        void RegisterPublisher(String processName, String processURL);
        void RegisterSubscriber(String processName, String processURL);
        void WriteIntoLog(String logMessage);
        void WriteIntoFullLog(String logMessage);
    }
}
