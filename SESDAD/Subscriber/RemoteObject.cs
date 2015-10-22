using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SESDAD.CommonTypes;

namespace SESDAD.Processes {
    public class SubscriberService : MarshalByRefObject, ISubscriberRemoteService {
        ISubscriber subscriber;

        public SubscriberService(ISubscriber newSubscriber) :
            base() {
            subscriber = newSubscriber;
        }

        public void Subscribe(String topicName) {
            subscriber.Subscribe(topicName);
        }
        public void Unsubscribe(String topicName) {
            subscriber.Unsubscribe(topicName);
        }

        public void DeliverEntry(Entry entry) {
            subscriber.DeliverEntry(entry);
        }

        public void Freeze() {
            subscriber.Freeze();
        }
        public void Unfreeze() {
            subscriber.Unfreeze();
        }
        public void Crash() {
            subscriber.Crash();
        }

        public void Ping() { }
    }
}
