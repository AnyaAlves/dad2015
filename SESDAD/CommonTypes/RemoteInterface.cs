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
        void Publish(ProcessHeader processHeader, Entry entry);

        void Subscribe(ProcessHeader processHeader, String topicName);
        void Unsubscribe(ProcessHeader processHeader, String topicName);

        void RegisterBroker(ProcessHeader processHeader);
        void RegisterPublisher(ProcessHeader processHeader);
        void RegisterSubscriber(ProcessHeader processHeader);
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
        void RegisterBroker(ProcessHeader processHeader);
        void RegisterPublisher(ProcessHeader processHeader);
        void RegisterSubscriber(ProcessHeader processHeader);
        void WriteIntoLog(String logMessage);
        void WriteIntoFullLog(String logMessage);
    }
}
