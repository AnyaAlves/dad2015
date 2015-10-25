using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SESDAD.CommonTypes;

namespace SESDAD.Processes {
    public class PublisherService : MarshalByRefObject, IPublisherRemoteService {
        IPublisher publisher;

        public PublisherService(IPublisher newPublisher) :
            base() {
            publisher = newPublisher;
        }

        public void Publish(String topicName, String content) {
            publisher.Publish(topicName, content);
        }

        public void Freeze() {
            publisher.Freeze();
        }
        public void Unfreeze() {
            publisher.Unfreeze();
        }
        public void Crash() {
            publisher.Crash();
        }

        public void Ping() { }
    }
}
