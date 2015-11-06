using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SESDAD.CommonTypes;

namespace SESDAD.Processes {


    public delegate IList<ProcessHeader> ProcessListDel(Topic topic);
    
    public class Topic : Node<Topic> {
        private IList<ProcessHeader> subscriberList;
        private IList<ProcessHeader> brokerList;

        public Topic(String topicName, Topic parent) :
            base (topicName, parent) {
            subscriberList = new List<ProcessHeader>();
            brokerList = new List<ProcessHeader>();
        }

        public IList<ProcessHeader> SubscriberList {
            get { return subscriberList; }
        }
        public IList<ProcessHeader> BrokerList {
            get { return brokerList; }
        }

        public IList<ProcessHeader> GetSubscriberList(Topic topic) {
            return topic.SubscriberList;
        }
        public IList<ProcessHeader> GetBrokerList(Topic topic) {
            return topic.BrokerList;
        }

        public void AddSubscriber(ProcessHeader subscriber) {
            subscriberList.Add(subscriber);
        }

        public void RemoveSubscriber(ProcessHeader subscriber) {
            subscriberList.Remove(subscriber);
        }

        public void AddBroker(ProcessHeader broker) {
            brokerList.Add(broker);
        }

        public void RemoveBroker(ProcessHeader broker) {
            brokerList.Remove(broker);
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

        public IList<ProcessHeader> GetProcessList(String topicPath, ProcessListDel processList) {
            IList<ProcessHeader> process = new List<ProcessHeader>();
            Topic child;
            var topics = topicPath.Split(new[] { '/' }, 2);
            String childName = topics[0];

            //if topic has *, append subscribers
            if (Children.TryGetValue("*", out child)) {
                process = processList(child);
            }
            //if topic doesn't have this child, return current list
            if (!Children.TryGetValue(childName, out child)) {
                return process;
            }
            //if current topic is the last, return subscribers list
            if (topics.Length == 1) {
                return process.Union(processList(child)).ToList();
            }
            return process.Union(child.GetProcessList(topics[1], processList)).ToList();
        }

        public IList<ProcessHeader> GetSubscriberList(String topicPath) {
            return GetProcessList(topicPath, new ProcessListDel(GetSubscriberList));
        }

        public IList<ProcessHeader> GetBrokerList(String topicPath) {
            return GetProcessList(topicPath, new ProcessListDel(GetBrokerList));
        }

        public bool HasProcesses() {
            return brokerList.Any() && subscriberList.Any();
        }
    }
}
