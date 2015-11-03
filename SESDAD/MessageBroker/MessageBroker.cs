using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Threading;

using SESDAD.CommonTypes;


namespace SESDAD.Processes {

    public class MessageBroker : GenericProcess, IMessageBroker {
        // States
        private RoutingPolicyType routingPolicy;
        private OrderingType ordering;
        // Tables
        private IDictionary<ProcessHeader, IMessageBrokerService> childBrokerList;
        private IDictionary<ProcessHeader, ISubscriberService> subscriberList;
        private Topic topicRoot;
        private Queue<Entry> requestBuffer;

        public MessageBroker(ProcessHeader processHeader) :
            base(processHeader) {
            childBrokerList = new Dictionary<ProcessHeader, IMessageBrokerService>();
            subscriberList = new Dictionary<ProcessHeader, ISubscriberService>();
            topicRoot = new Topic("", null);
            requestBuffer = new Queue<Entry>();
        }

        public RoutingPolicyType RoutingPolicy {
            set { routingPolicy = value; }
        }
        public OrderingType Ordering {
            set { ordering = value; }
        }

        public void AddSubscriber(ProcessHeader subscriberHeader) {
            //get subscriber remote object
            ISubscriberService newSubscriber =
                (ISubscriberService)Activator.GetObject(
                       typeof(ISubscriberService),
                       subscriberHeader.ProcessURL);

            //add the new subscriber
            subscriberList.Add(subscriberHeader, newSubscriber);
        }


        public void MakeSubscription(ProcessHeader subscriberHeader, String topicName) {
            Topic topic = topicRoot.GetSubtopic(topicName);
            //if current broker isn't root root and topic is new (hasn't been sent), send to parent 
            if (routingPolicy == RoutingPolicyType.FILTERING &&
                    ParentBroker == null &&
                    !topic.HasProcesses()) {
                ParentBroker.SpreadSubscription(Header, topicName);
            }
            topic.AddSubscriber(subscriberHeader);
        }


        public void RemoveSubscription(ProcessHeader subscriberHeader, String topicName) {
            Topic topic = topicRoot.GetSubtopic(topicName);
            topic.RemoveSubscriber(subscriberHeader);
            //spread unsub
        }
        public void AckDelivery(ProcessHeader subscriberHeader) { }

        public void SubmitEntry(Entry entry) {
            MulticastEntry(Header, entry);
        }

        public void AddChildBroker(ProcessHeader childBrokerHeader) {
            IMessageBrokerService child = (IMessageBrokerService)Activator.GetObject(
                typeof(IMessageBrokerService),
                childBrokerHeader.ProcessURL);
            childBrokerList.Add(childBrokerHeader, child);
        }

        public override void ConnectToParentBroker(String parentBrokerURL) {
            base.ConnectToParentBroker(parentBrokerURL);
            ParentBroker.RegisterChildBroker(Header);
        }

        public void SpreadSubscription(ProcessHeader brokerHeader, String topicName) {
            Topic topic = topicRoot.GetSubtopic(topicName);

            if (ParentBroker != null && !topic.HasProcesses()) {
                ParentBroker.SpreadSubscription(Header, topicName);
                topic.AddBroker(brokerHeader);
            }
        }

        public void MulticastEntry(ProcessHeader senderBrokerHeader, Entry entry) {
            requestBuffer.Enqueue(entry);
            ThreadStart ts = new ThreadStart(this.ForwardEntries);
            Thread t = new Thread(ts);
            t.Start();

            IList<ProcessHeader> brokerList;

            if (routingPolicy == RoutingPolicyType.FLOODING) {
                brokerList = childBrokerList.Keys.ToList();
            }
            else {// if (routingPolicy == RoutingPolicyType.FILTERING) {
                brokerList = topicRoot.GetBrokerList(entry.TopicName);
            }

            //if current broker isn't root AND parent isn't sender, send to parent
            if (ParentBroker != null && !ParentBroker.Header.Equals(senderBrokerHeader)) {
                ParentBroker.MulticastEntry(Header, entry);
            }
            //send to brokerlist
            foreach (ProcessHeader childBroker in brokerList) {
                childBrokerList[childBroker].MulticastEntry(Header, entry);
            }
        }

        public void ForwardEntries() {
            Entry entry = requestBuffer.Dequeue();
            String topicName = entry.TopicName;
            IList<ProcessHeader> topicSubscriberList = topicRoot.GetSubscriberList(topicName);

            foreach (ProcessHeader subscriber in topicSubscriberList) {
                subscriberList[subscriber].DeliverEntry(entry);
            }
        }

        public override String ToString() {
            return
                "**********************************************" + Environment.NewLine +
                " Message Broker :" + Environment.NewLine + Environment.NewLine +
                base.ToString() +
                "**********************************************" + Environment.NewLine +
                " Subscribers :" + Environment.NewLine + Environment.NewLine +
                String.Join(Environment.NewLine, subscriberList.Keys) +
                "**********************************************" + Environment.NewLine +
                " Children :" + Environment.NewLine + Environment.NewLine +
                String.Join(Environment.NewLine, childBrokerList.Keys) +
                "**********************************************" + Environment.NewLine;
        }
    }

    class Program {
        static void Main(string[] args) {
            ProcessHeader processHeader = new ProcessHeader(args[0], ProcessType.BROKER, args[1], args[2]);
            MessageBroker process = new MessageBroker(processHeader);

            process.LaunchService<MessageBrokerService, IMessageBroker>(((IMessageBroker)process));
            if (args.Length == 4) {
                process.ConnectToParentBroker(args[3]);
            }
            Console.WriteLine(process);
            Console.ReadLine();
        }
    }
}