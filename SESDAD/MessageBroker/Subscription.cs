using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SESDAD.CommonTypes;

namespace SESDAD.Processes {
    using PublisherSeqNumberTable = IDictionary<ProcessHeader,int>;
    
    class Subscription : Node<Subscription> {
        private String topicName;
        private IDictionary<ISubscriberService, PublisherSeqNumberTable> subscriberList;
        private IList<IMessageBrokerService> brokerList;

        public Subscription(String newTopicName) :
            base (null) {
            topicName = newTopicName;
            subscriberList = new Dictionary<ISubscriberService,PublisherSeqNumberTable>();
        }

        public AddSubscriber(ISubscriberService subscriber) {
            
        }
            return null;
        
        }

        public override Add()

    }
}
