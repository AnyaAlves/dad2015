using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SESDAD.Commons;

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

        public IList<ProcessHeader> GetProcessList(String topicPath, ProcessListDel getProcessList) {
            IList<ProcessHeader> processList = new List<ProcessHeader>();
            Topic subtopic;
            //split whole topic string into subtopic|suffix
            var topics = topicPath.Split(new[] { '/' }, 2);

            //if topic has this subtopic
            if (Children.TryGetValue(topics[0], out subtopic)) {
                //if next topic is the last, get processes interested in that subtopic
                if (topics.Length == 1) {
                    processList = getProcessList(subtopic);
                }
                else { //recurse with suffix
                    processList = subtopic.GetProcessList(topics[1], getProcessList);
                }
            }
            //append processes interested in prefix
            if (Children.TryGetValue("*", out subtopic)) {
                processList = processList.Union(getProcessList(subtopic)).ToList();
            }
            return processList;
        }

        public IList<ProcessHeader> GetSubscriberList(String topicPath) {
            return GetProcessList(topicPath, new ProcessListDel(GetSubscriberList));
        }

        public IList<ProcessHeader> GetBrokerList(String topicPath) {
            return GetProcessList(topicPath, new ProcessListDel(GetBrokerList));
        }

        public bool AlreadySubscribed(ProcessHeader broker) {
            return brokerList.Any() || subscriberList.Any();
        }
    }
}
