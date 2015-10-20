using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SESDAD.CommonTypes {
    public interface IProcessRemoteService {
        void ConnectToPuppetMaster(String puppetMasterURL);
    }

    public interface IBrokerRemoteService : IProcessRemoteService {
        void Publish(String processName, String processURL, Entry entry);

        void Subscribe(String processName, String processURL, String topicName);
        void Unsubscribe(String processName, String processURL, String topicName);

        void RegisterBroker(String processName, String processURL);
    }

    public interface ISubscriberRemoteObject : IProcessRemoteService {
        void DeliverEntry(Entry entry);
    }

    public interface IPublisherRemoteObject : IProcessRemoteService {

    }

    public interface IAdministratorService {
        void ConfirmConnection(String processName);
        void WriteIntoLog(String logMessage);
    }
}
