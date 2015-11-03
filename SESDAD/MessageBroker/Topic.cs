using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SESDAD.CommonTypes;

namespace SESDAD.Processes {
    using PublisherTable = Dictionary<ProcessHeader,int>;
    
    public class Topic : Node<Topic> {
        private IDictionary<ProcessHeader, PublisherTable> subscriberList;
        private IList<IMessageBrokerService> brokerList;

        public Topic(String topicName, Topic parent) :
            base (topicName, parent) {
            subscriberList = new Dictionary<ProcessHeader,PublisherTable>();
            brokerList = new List<IMessageBrokerService>();
        }

        public IList<ProcessHeader> SubscriberList {
            get {
                return subscriberList.Keys.ToList();
            }
        }

        public void AddSubscriber(ProcessHeader subscriber) {
            subscriberList.Add(subscriber, new PublisherTable());
        }

        public void RemoveSubscriber(ProcessHeader subscriber) {
            subscriberList.Remove(subscriber);
        }

        public void AddBroker(IMessageBrokerService broker) {
            brokerList.Add(broker);
        }

        public Topic GetSubtopic(String topicPath) {
            Topic child;
            var topics = topicPath.Split(new [] { '/' }, 2);
            String childName = topics[0];

            //if topic doesn't have this child, create it
            if (!Children.TryGetValue(childName, out child)) {
                child = new Topic(childName, this);
            }
            //if current topic is the last, no need to search the rest of the tree
            if (topics.Length == 1) {
                return child;
            }
            return child.GetSubtopic(topics[1]);
        }

        // /a/aa/b

        public IList<ProcessHeader> GetSubscriberList(String topicPath) {
            IList<ProcessHeader> subscribers = new List<ProcessHeader>();
            Topic child;
            var topics = topicPath.Split(new[] { '/' }, 2);
            String childName = topics[0];

            //if topic has *, append subscribers
            if (Children.TryGetValue("*", out child)) {
                subscribers = child.SubscriberList;
            }
            //if topic doesn't have this child, return current list
            if (!Children.TryGetValue(childName, out child)) {
                return subscribers;
            }
            //if current topic is the last, return subscribers list
            if (topics.Length == 1) {
                return child.SubscriberList;
            }
            return subscribers.Concat(child.GetSubscriberList(topics[1])).ToList();
        }

        public IList<IMessageBrokerService> GetBrokerList(String topicPath) {
            IList<IMessageBrokerService> brokers = new List<IMessageBrokerService>();
            Topic child;
            var topics = topicPath.Split(new[] { '/' }, 2);
            String childName = topics[0];

            //if topic has *, append subscribers
            if (Children.TryGetValue("*", out child)) {
                brokers = child.brokerList;
            }
            //if topic doesn't have this child, return current list
            if (!Children.TryGetValue(childName, out child)) {
                return brokers;
            }
            //if current topic is the last, return subscribers list
            if (topics.Length == 1) {
                return child.brokerList;
            }
            return brokers.Concat(child.GetBrokerList(topics[1])).ToList();
        }

        public bool HasProcesses() {
            return (brokerList != null) && (subscriberList != null);
        }
    }
}
