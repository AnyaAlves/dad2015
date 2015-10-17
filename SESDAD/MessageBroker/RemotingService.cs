using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SESDAD.CommonTypes;

namespace SESDAD.MessageBroker {
    public class BrokerService : MarshalByRefObject, IBrokerService {
        MessageBroker broker;

        public BrokerService(MessageBroker broker) : base() {
            this.broker = broker;
        }

        public void Publish(String processName, String processURL, String entry) {
            broker.ForwardEntry(processName, processURL, entry);
        }

        public void Subscribe(String processName, String processURL, String topicName) {
            broker.registerSubscription(processName, processURL, topicName);
            Console.WriteLine("SUB");
        }

        public void Unsubscribe(String processName, String processURL, String topicName) {
            broker.removeSubscription(processName, processURL, topicName);
            Console.WriteLine("PUB");
        }
    }
}
