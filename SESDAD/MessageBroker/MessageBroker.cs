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

        public MessageBroker(ProcessHeader newProcessHeader) :
            base(newProcessHeader) {
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
            PuppetMaster.RegisterBroker(ProcessHeader);
            Console.WriteLine("Connected to " + newPuppetMasterURL);
        }
        public override void ConnectToParentBroker(String newParentURL) {
            base.ConnectToParentBroker(newParentURL);
            ParentBroker.RegisterBroker(ProcessHeader);
            Console.WriteLine("Connected to " + newParentURL);
        }
        public void RegisterBroker(String newProcessURL) {
            IBrokerRemoteService child = (IBrokerRemoteService)Activator.GetObject(
                typeof(IBrokerRemoteService),
                newProcessURL);
            brokerTable.Add(child.ProcessName, child);
            Console.WriteLine("Connected to " + newProcessURL);
        }
        public void RegisterBroker(ProcessHeader processHeader) {
            IBrokerRemoteService child = (IBrokerRemoteService)Activator.GetObject(
                typeof(IBrokerRemoteService),
                processHeader.ProcessURL);
            brokerTable.Add(processHeader.ProcessName, child);
            Console.WriteLine("Connected to " + processHeader.ProcessURL);
        }
        public void RegisterPublisher(ProcessHeader processHeader) {
            IPublisherRemoteService child = (IPublisherRemoteService)Activator.GetObject(
                typeof(IPublisherRemoteService),
                processHeader.ProcessURL);
            publisherTable.Add(processHeader.ProcessName, child);
            Console.WriteLine("Connected to " + processHeader.ProcessURL);
        }
        public void RegisterSubscriber(ProcessHeader processHeader) {
            ISubscriberRemoteService child = (ISubscriberRemoteService)Activator.GetObject(
                typeof(ISubscriberRemoteService),
                processHeader.ProcessURL);
            subscriberTable.Add(processHeader.ProcessName, child);
            Console.WriteLine("Connected to " + processHeader.ProcessURL);
        }

        public void registerSubscription(ProcessHeader processHeader, String topicName) {
            SubsciberTable subscriberList;

            //get subscriber remote object
            ISubscriberRemoteService newSubscriber =
                (ISubscriberRemoteService)Activator.GetObject(
                       typeof(ISubscriberRemoteService),
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
                foreach (KeyValuePair<String, ISubscriberRemoteService> subscriber in subscriberList.ToList()) {
                    subscriber.Value.DeliverEntry(entry);
                }
            }
            //eventrouter.broadcast
            Console.WriteLine("Forwarding entry to all subscribers");



            PuppetMaster.WriteIntoFullLog("BroEvent " + ProcessName + ", " + processHeader.ProcessName + ", " + entry.TopicName + ", event-number");
        }

        public void NotifyParent() { }

        public void Debug() {
            String debugMessage =
                "**********************************************" + Environment.NewLine +
                "* Hello, I'm a MessageBroker. Here's my info:" + Environment.NewLine +
                "*" + Environment.NewLine +
                "* Site Name:    " + SiteName + Environment.NewLine +
                "* Process Name: " + ProcessName + Environment.NewLine +
                "* Process URL:  " + ProcessURL + Environment.NewLine +
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
            ProcessHeader processHeader = new ProcessHeader(args[0], ProcessType.SUBSCRIBER, args[1], args[2]);
            MessageBroker broker = new MessageBroker(processHeader);

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