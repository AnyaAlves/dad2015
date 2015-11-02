using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Threading;

using SESDAD.CommonTypes;


namespace SESDAD.Processes {

    using PublisherTable = Dictionary<ProcessHeader, int>;
    using IPublisherTable = IDictionary<ProcessHeader, int>;
    using SubsciberTable = Dictionary<String, ISubscriberService>;
    using ISubsciberTable = IDictionary<String, ISubscriberService>;
    using ChildBrokerTable = Dictionary<ProcessHeader, IMessageBrokerService>;
    using IChildBrokerTable = IDictionary<ProcessHeader, IMessageBrokerService>;


    public class MessageBroker : GenericProcess, IMessageBroker {
        // States
        private RoutingPolicyType routingPolicy;
        private OrderingType ordering;
        // Tables
        private IDictionary<String, SubsciberTable> subscriptions;
        private IChildBrokerTable childBrokerTable;
        private IPublisherTable publisherTable;
        private ISubsciberTable subscriberTable;

        public MessageBroker(ProcessHeader newProcessHeader) :
            base(newProcessHeader) {
            subscriptions = new Dictionary<String, SubsciberTable>();
            childBrokerTable = new ChildBrokerTable();
            publisherTable = new PublisherTable();
            subscriberTable = new SubsciberTable();
        }

        public RoutingPolicyType RoutingPolicy {
            set { routingPolicy = value; }
        }
        public OrderingType Ordering {
            set { ordering = value; }
        }

        public void AddSubscriber(ProcessHeader processHeader) {
            //get subscriber remote object
            ISubscriberService newSubscriber =
                (ISubscriberService)Activator.GetObject(
                       typeof(ISubscriberService),
                       processHeader.ProcessURL);

            //add the new subscriber
            subscriberTable.Add(processHeader.ProcessName, newSubscriber);
            Console.WriteLine(String.Join(" " ,subscriberTable.Keys));
        }
        public void AddSubscription(ProcessHeader processHeader, String topicName) {
            //  fullPath-> /a/aa/aaa
            // nodes = split(/)
            // for int i=0;i<nodes.length-1;i++
            //node[i].Add(node[i+1])


            //if there are no subs to that topic, create a new list of subscribers
            SubsciberTable subscription;
            if (!subscriptions.TryGetValue(topicName, out subscription)) {
                subscriberTable = new SubsciberTable();
                subscriptions.Add(topicName, subscription);
            }
        }
        public void RemoveSubscription(ProcessHeader processHeader, String topicName) {
            SubsciberTable subscriberList;
            //get subscriber by name and remove it from the list
            if (subscriptions.TryGetValue(topicName, out subscriberList)) {
                subscriberList.Remove(processHeader.ProcessName);
            }
            //notify parent to update
            Console.WriteLine("Removed subcriber: " + processHeader.ProcessName);
        }
        //public void Ack(ProcessHeader processHeader) { }

        public void ForwardEntry(ProcessHeader processHeader, Entry entry) {
            SubsciberTable subscriberList;
            String topicName = entry.TopicName;

            switch (routingPolicy) {
                case RoutingPolicyType.FLOODING:
                    // Missing Table (I have no Idea!!!!!)
                    Console.WriteLine(String.Join(" ", subscriberTable.Keys));
                    foreach (ISubscriberService subscriber in subscriberTable.Values) {
                        subscriber.DeliverEntry(entry);
                    }
                    break;
                case RoutingPolicyType.FILTER:
                    if (subscriptions.TryGetValue(topicName, out subscriberList)) {
                        foreach (ISubscriberService subscriber in subscriberList.Values) {
                            subscriber.DeliverEntry(entry);
                        }
                    }
                    break;
            }
        }

        public void AddChildBroker(String newProcessURL) {
            IMessageBrokerService child = (IMessageBrokerService)Activator.GetObject(
                typeof(IMessageBrokerService),
                newProcessURL);
            childBrokerTable.Add(child.ProcessHeader, child);
        }
        public void AddChildBroker(ProcessHeader processHeader) {
            IMessageBrokerService child = (IMessageBrokerService)Activator.GetObject(
                typeof(IMessageBrokerService),
                processHeader.ProcessURL);
            childBrokerTable.Add(processHeader, child);
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
            process.Debug();

            Console.ReadLine();
        }
    }
}