using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SESDAD.CommonTypes {
    public interface IGenericProcessService {
        ProcessHeader Header { get; }

        void ConnectToPuppetMaster(String puppetMasterURL);
        void ConnectToParentBroker(String parentbrokerURL);

        void ForceFreeze();
        void ForceUnfreeze();
        void ForceCrash();

        String GetStatus();
    }

    public interface IMessageBrokerService : IGenericProcessService {
        ///<summary>
        /// Broker Remote Service interface routing policy
        ///</summary>
        RoutingPolicyType RoutingPolicy { set; }
        ///<summary>
        /// Broker Remote Service interface ordering
        ///</summary>
        OrderingType Ordering { set; }

        //subscriber->broker
        void RegisterSubscriber(ProcessHeader subscriberHeader);
        void Subscribe(ProcessHeader subscriberHeader, String topicName);
        void Unsubscribe(ProcessHeader subscriberHeader, String topicName);
        void AckDelivery(ProcessHeader subscriberHeader, ProcessHeader publisherHeader);

        //publisher->broker
        void Publish(Entry entry);

        //child->broker
        void RegisterChildBroker(ProcessHeader childBrokerHeader);
        //broker->parent
        void SpreadSubscription(ProcessHeader brokerHeader, String topicName);
        //broker->brokers
        void MulticastEntry(ProcessHeader senderBrokerHeader, Entry entry);

    }

    public interface ISubscriberService : IGenericProcessService {
        void DeliverEntry(Entry entry);

        void ForceSubscribe(String topicName);
        void ForceUnsubscribe(String topicName);
    }

    public interface IPublisherService : IGenericProcessService {
        //void Ack();

        void ForcePublish(String topicName, String content);
    }

    public interface IPuppetMasterService {
        void WriteIntoLog(String logMessage);
        void WriteIntoFullLog(String logMessage);
    }
}
