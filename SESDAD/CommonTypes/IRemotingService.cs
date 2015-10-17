using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SESDAD.CommonTypes {
    public interface IBrokerPubService {
        void Publish(String processName, String processURL, String entry);
    }

    public interface IBrokerSubService {
        void Subscribe(String processName, String processURL, String topicName);
        void Unsubscribe(String processName, String processURL, String topicName);
    }

    public interface ISubscriberService {
        void DeliverEntry(String entry);
    }
}
