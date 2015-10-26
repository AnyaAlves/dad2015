using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Net.Sockets;
using System.Threading;

using SESDAD.CommonTypes;


namespace SESDAD.Processes {

    using SubsciberTable = Dictionary<String, ISubscriberService>; //typedef

    public class MessageBroker : Process, IMessageBroker {

        EventRouter eventRouter;
        private IDictionary<String, SubsciberTable> subscriptions;
        private IDictionary<String, IBrokerService> brokerTable;
        private IDictionary<String, IPublisherService> publisherTable;
        private IDictionary<String, ISubscriberService> subscriberTable;

        // States
        private RoutingPolicyType routingPolicy;
        private OrderingType ordering;

        public MessageBroker(ProcessHeader newProcessHeader) :
            base(newProcessHeader) {
            eventRouter = new EventRouter();
            subscriptions = new Dictionary<String, SubsciberTable>();
            brokerTable = new Dictionary<String, IBrokerService>();
            publisherTable = new Dictionary<String, IPublisherService>();
            subscriberTable = new Dictionary<String, ISubscriberService>();
        }

        ///<summary>
        /// Broker Remote Service routing policy
        ///</summary>
        public RoutingPolicyType RoutingPolicy {
            set { routingPolicy = value; }
        }
        ///<summary>
        /// Broker Remote Service ordering
        ///</summary>
        public OrderingType Ordering {
            set { ordering = value; }
        }

        public override void LaunchService() {
            TcpConnect();
            RemotingServices.Marshal(
                new MessageBrokerService((IMessageBroker)this),
                serviceName,
                typeof(MessageBrokerService));
        }

        public void RegisterBroker(String newProcessURL) {
            IBrokerService child = (IBrokerService)Activator.GetObject(
                typeof(IBrokerService),
                newProcessURL);
            brokerTable.Add(child.ProcessName, child);
            Console.WriteLine("Connected to " + newProcessURL);
        }
        public void RegisterBroker(ProcessHeader processHeader) {
            IBrokerService child = (IBrokerService)Activator.GetObject(
                typeof(IBrokerService),
                processHeader.ProcessURL);
            brokerTable.Add(processHeader.ProcessName, child);
            Console.WriteLine("Connected to " + processHeader.ProcessURL);
        }
        public void RegisterPublisher(ProcessHeader processHeader) {
            IPublisherService child = (IPublisherService)Activator.GetObject(
                typeof(IPublisherService),
                processHeader.ProcessURL);
            publisherTable.Add(processHeader.ProcessName, child);
            Console.WriteLine("Connected to " + processHeader.ProcessURL);
        }
        public void RegisterSubscriber(ProcessHeader processHeader) {
            ISubscriberService child = (ISubscriberService)Activator.GetObject(
                typeof(ISubscriberService),
                processHeader.ProcessURL);
            subscriberTable.Add(processHeader.ProcessName, child);
            Console.WriteLine("Connected to " + processHeader.ProcessURL);
        }

        public void registerSubscription(ProcessHeader processHeader, String topicName) {
            SubsciberTable subscriberList;

            //get subscriber remote object
            ISubscriberService newSubscriber =
                (ISubscriberService)Activator.GetObject(
                       typeof(ISubscriberService),
                       processHeader.ProcessURL);
            //if there are no subs to that topic, create a new list of subscribers
            if (!subscriptions.TryGetValue(topicName, out subscriberList)) {
                subscriberList = new SubsciberTable();
                subscriptions.Add(topicName, subscriberList);
            }
            //add the new subscriber
            subscriberList.Add(processHeader.ProcessName, newSubscriber);
            //notify parent to update
            Console.WriteLine("New subcriber: " + processHeader.ProcessName);
        }
        public void removeSubscription(ProcessHeader processHeader, String topicName) {
            SubsciberTable subscriberList;
            //get subscriber by name and remove it from the list
            if (subscriptions.TryGetValue(topicName, out subscriberList)) {
                subscriberList.Remove(processHeader.ProcessName);
            }
            //notify parent to update
            Console.WriteLine("Removed subcriber: " + processHeader.ProcessName);
        }
        public void ForwardEntry(ProcessHeader processHeader, Entry entry) {
            SubsciberTable subscriberList;
            String topicName = entry.TopicName;

            if (subscriptions.TryGetValue(topicName, out subscriberList)) {
                foreach (KeyValuePair<String, ISubscriberService> subscriber in subscriberList.ToList()) {
                    subscriber.Value.DeliverEntry(entry);
                }
            }
            //eventrouter.broadcast
            Console.WriteLine("Forwarding entry to all subscribers");
        }

        public void NotifyParent() { }
    }

    class Program {
        static void Main(string[] args) {
            ProcessHeader processHeader = new ProcessHeader(args[0], ProcessType.BROKER, args[1], args[2]);
            MessageBroker process = new MessageBroker(processHeader);

            process.LaunchService();
            if (args.Length == 4) {
                process.ConnectToParentBroker(args[3]);
            }
            process.Debug();

            Console.ReadLine();
        }
    }
}