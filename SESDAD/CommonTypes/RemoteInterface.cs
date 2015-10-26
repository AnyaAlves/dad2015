using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SESDAD.CommonTypes {
    public interface IGenericProcessService {
        void ConnectToPuppetMaster(String puppetMasterURL);
        void ConnectToParentBroker(String parentbrokerURL);

        void ForceFreeze();
        void ForceUnfreeze();
        void ForceCrash();

        void TryPing();
    }

    public interface IBrokerService : IGenericProcessService {
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

    public interface ISubscriberService : IGenericProcessService {
        void DeliverEntry(Entry entry);

        void ForceSubscribe(String topicName);
        void ForceUnsubscribe(String topicName);
    }

    public interface IPublisherService : IGenericProcessService {
        void ForcePublish(String topicName, String content);
    }

    public interface IPuppetMasterService {
        void WriteIntoLog(String logMessage);
        void WriteIntoFullLog(String logMessage);
    }
}
