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

    using SubsciberTable = Dictionary<String, ISubscriberRemoteService>; //typedef

    public class MessageBroker : Process, IMessageBroker {

        EventRouter eventRouter;
        private IDictionary<String, SubsciberTable> subscriptions;
        private IDictionary<String, IBrokerRemoteService> brokerTable;
        private IDictionary<String, IPublisherRemoteService> publisherTable;
        private IDictionary<String, ISubscriberRemoteService> subscriberTable;

        // States
        private RoutingPolicyType routingPolicy;
        private OrderingType ordering;

        public MessageBroker(
            String newProcessName,
            String newSiteName,
            String newProcessURL) :
            base(newProcessName, newSiteName, newProcessURL) {
            eventRouter = new EventRouter();
            subscriptions = new Dictionary<String, SubsciberTable>();
            brokerTable = new Dictionary<String, IBrokerRemoteService>();
            publisherTable = new Dictionary<String, IPublisherRemoteService>();
            subscriberTable = new Dictionary<String, ISubscriberRemoteService>();
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

        public override void ServiceInit() {
            base.ServiceInit();
            //make remote service available
            RemotingServices.Marshal(
                new BrokerRemoteService((IMessageBroker)this),
                serviceName,
                typeof(BrokerRemoteService));
        }

        public override void ConnectToPuppetMaster(String newPuppetMasterURL) {
            base.ConnectToPuppetMaster(newPuppetMasterURL);
            PuppetMaster.RegisterBroker(processName, processURL);
            Console.WriteLine("Connected to " + newPuppetMasterURL);
        }
        public override void ConnectToParentBroker(String newParentURL) {
            base.ConnectToParentBroker(newParentURL);
            ParentBroker.RegisterBroker(processName, processURL);
            Console.WriteLine("Connected to " + newParentURL);
        }
        public void RegisterBroker(String newProcessURL) {
            IBrokerRemoteService child = (IBrokerRemoteService)Activator.GetObject(
                typeof(IBrokerRemoteService),
                newProcessURL);
            brokerTable.Add(child.ProcessName, child);
            Console.WriteLine("Connected to " + newProcessURL);
        }
        public void RegisterBroker(String newProcessName, String newProcessURL) {
            IBrokerRemoteService child = (IBrokerRemoteService)Activator.GetObject(
                typeof(IBrokerRemoteService),
                newProcessURL);
            brokerTable.Add(newProcessName, child);
            Console.WriteLine("Connected to " + newProcessURL);
        }
        public void RegisterPublisher(String newProcessName, String newProcessURL) {
            IPublisherRemoteService child = (IPublisherRemoteService)Activator.GetObject(
                typeof(IPublisherRemoteService),
                newProcessURL);
            publisherTable.Add(newProcessName, child);
            Console.WriteLine("Connected to " + newProcessURL);
        }
        public void RegisterSubscriber(String newProcessName, String newProcessURL) {
            ISubscriberRemoteService child = (ISubscriberRemoteService)Activator.GetObject(
                typeof(ISubscriberRemoteService),
                newProcessURL);
            subscriberTable.Add(newProcessName, child);
            Console.WriteLine("Connected to " + newProcessURL);
        }

        public void registerSubscription(String processName, String processURL, String topicName) {
            SubsciberTable subscriberList;

            //get subscriber remote object
            ISubscriberRemoteService newSubscriber =
                (ISubscriberRemoteService)Activator.GetObject(
                       typeof(ISubscriberRemoteService),
                       processURL);
            //if there are no subs to that topic, create a new list of subscribers
            if (!subscriptions.TryGetValue(topicName, out subscriberList)) {
                subscriberList = new SubsciberTable();
                subscriptions.Add(topicName, subscriberList);
            }
            //add the new subscriber
            subscriberList.Add(processName, newSubscriber);
            //notify parent to update
            Console.WriteLine("New subcriber: " + processName);
        }
        public void removeSubscription(String processName, String processURL, String topicName) {
            SubsciberTable subscriberList;
            //get subscriber by name and remove it from the list
            if (subscriptions.TryGetValue(topicName, out subscriberList)) {
                subscriberList.Remove(processName);
            }
            //notify parent to update
            Console.WriteLine("Removed subcriber: " + processName);
        }
        public void ForwardEntry(String processName, String processURL, Entry entry) {
            SubsciberTable subscriberList;
            String topicName = entry.TopicName;

            if (subscriptions.TryGetValue(topicName, out subscriberList)) {
                foreach (KeyValuePair<String, ISubscriberRemoteService> subscriber in subscriberList.ToList()) {
                    subscriber.Value.DeliverEntry(entry);
                }
            }
            //eventrouter.broadcast
            Console.WriteLine("Forwarding entry to all subscribers");



            PuppetMaster.WriteIntoFullLog("BroEvent " + ProcessName + ", " + processName + ", " + entry.TopicName + ", event-number");
        }

        public void NotifyParent() { }

        public void Debug() {
            String debugMessage =
                "**********************************************" + Environment.NewLine +
                "* Hello, I'm a MessageBroker. Here's my info:" + Environment.NewLine +
                "*" + Environment.NewLine +
                "* Site Name:    " + siteName + Environment.NewLine +
                "* Process Name: " + processName + Environment.NewLine +
                "* Process URL:  " + processURL + Environment.NewLine +
                "*" + Environment.NewLine +
                "* Service Name: " + serviceName + Environment.NewLine +
                "* Service Port: " + portNumber + Environment.NewLine +
                "*" + Environment.NewLine +
                "* Parent Name:  " + ParentBrokerName + Environment.NewLine +
                "* Parent URL:   " + ParentBrokerURL + Environment.NewLine +
                "**********************************************" + Environment.NewLine;
            Console.Write(debugMessage);
        }
    }

    class Program {
        static void Main(string[] args) {
            MessageBroker broker = new MessageBroker(args[0], args[1], args[2]);

            broker.ServiceInit();
            broker.ConnectToPuppetMaster(args[3]);
            if (args.Length == 5) {
                broker.ConnectToParentBroker(args[4]);
            }
            broker.Debug();

            Console.ReadLine();
        }
    }
}