using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Threading;

using SESDAD.CommonTypes;


namespace SESDAD.Processes {

    public class MessageBroker : GenericProcess, IMessageBroker {
        private Topic topicRoot;
        // States
        private RoutingPolicyType routingPolicy;
        // Tables
        private IDictionary<ProcessHeader, ISubscriberService> subscriberList;
        private IDictionary<ProcessHeader, IMessageBrokerService> childBrokerList;
        EntryBufferManager bufferManager;

        public MessageBroker(ProcessHeader processHeader) :
            base(processHeader) {
            topicRoot = new Topic("", null);
            subscriberList = new Dictionary<ProcessHeader, ISubscriberService>();
            childBrokerList = new Dictionary<ProcessHeader, IMessageBrokerService>();
            bufferManager = new EntryBufferManager();
        }

        public RoutingPolicyType RoutingPolicy {
            set { routingPolicy = value; }
        }
        public OrderingType Ordering {
            set { bufferManager.Ordering = value; }
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
            if (ParentBroker != null && !topic.HasProcesses()) {
                ParentBroker.SpreadSubscription(Header, topicName);
            }
            topic.AddSubscriber(subscriberHeader);
        }


        public void RemoveSubscription(ProcessHeader subscriberHeader, String topicName) {
            Topic topic = topicRoot.GetSubtopic(topicName);
            topic.RemoveSubscriber(subscriberHeader);
            //spread unsub
        }

        public void AckDelivery(ProcessHeader subscriberHeader, ProcessHeader publisherHeader) {
            bufferManager.SendPendingEntry(subscriberHeader, publisherHeader, this);
        }

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
            }
            topic.AddBroker(brokerHeader);
        }

        public void MulticastEntry(ProcessHeader senderBrokerHeader, Entry entry) {
            bufferManager.InsertIntoInputBuffer(entry);
            Console.WriteLine(bufferManager);
            Console.WriteLine("Sender: " + senderBrokerHeader.ProcessName);
            ThreadStart ts = new ThreadStart(this.ForwardEntries);
            Thread t = new Thread(ts);
            t.Start();

            IList<ProcessHeader> brokerList = null;

            if (routingPolicy == RoutingPolicyType.FLOOD) {
                brokerList = childBrokerList.Keys.ToList();
            }
            else if (routingPolicy == RoutingPolicyType.FILTER) {
                brokerList = topicRoot.GetBrokerList(entry.TopicName);
            }
            brokerList.Remove(senderBrokerHeader);

            Console.WriteLine("broker List: \n" + String.Join("\n", brokerList));

            //if current broker isn't root AND parent isn't sender, send to parent
            if (ParentBroker != null && !ParentBroker.Header.Equals(senderBrokerHeader)) {
                ParentBroker.MulticastEntry(Header, entry);
            }
            //send to brokerlist
            foreach (ProcessHeader childBroker in brokerList) {
                childBrokerList[childBroker].MulticastEntry(Header, entry);
            }
        }

        public void SendEntry(ProcessHeader subscriber, Entry entry) {
            subscriberList[subscriber].DeliverEntry(entry);        
        }



        public void ForwardEntries() {
            Entry entry = bufferManager.GetEntry();
            IList<ProcessHeader> topicSubscriberList = topicRoot.GetSubscriberList(entry.TopicName);

            foreach (ProcessHeader subscriber in topicSubscriberList) {
                SendEntry(subscriber, entry);
                bufferManager.MoveToPendingDeliveryBuffer(subscriber, entry);
            }
        }

        public override String ToString() {
            String nl = Environment.NewLine;

            return
                "**********************************************" + nl +
                " Message Broker :" + nl +
                base.ToString() + nl +
                "**********************************************" + nl +
                " Subscribers :" + nl +
                String.Join(nl, subscriberList.Keys) + nl +
                "**********************************************" + nl +
                " Children :" + nl +
                String.Join(nl, childBrokerList.Keys) + nl +
                "**********************************************" + nl;
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