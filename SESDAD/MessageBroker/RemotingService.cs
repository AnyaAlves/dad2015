using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SESDAD.CommonTypes;

namespace SESDAD.MessageBroker {
    public class BrokerPubService : MarshalByRefObject, IBrokerPubService {
        MessageBroker broker;

        public BrokerPubService(MessageBroker broker) {
            this.broker = broker;
        }

        public void Publish(String processName, String processURL, String entry) {
            broker.ForwardEntry(processName, processURL, entry);
        }
    }

    public class BrokerSubService : MarshalByRefObject, IBrokerSubService {
        MessageBroker broker;

        public BrokerSubService(MessageBroker broker) {
            this.broker = broker;
        }

        public void Subscribe(String processName, String processURL, String topicName) {
            broker.registerSubscription(processName, processURL, topicName);
        }

        public void Unsubscribe(String processName, String processURL, String topicName) {
            broker.removeSubscription(processName, processURL, topicName);
        }
    }
}
