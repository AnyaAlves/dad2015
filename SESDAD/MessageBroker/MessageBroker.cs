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
        private IDictionary<String, IBrokerRemoteService> childrenBrokerTable;

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
            childrenBrokerTable = new Dictionary<String, IBrokerRemoteService>();
        }

        ///<summary>
        /// Broker Remote Service routing policy
        ///</summary>
        public RoutingPolicyType RoutingPolicy
        {
            set { routingPolicy = value; }
        }
        ///<summary>
        /// Broker Remote Service ordering
        ///</summary>
        public OrderingType Ordering
        {
            set { ordering = value; }
        }

        public override void Connect() {
            base.Connect();
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
            ParentBroker.RegisterBroker(processURL);
            Console.WriteLine("Connected to " + newParentURL);
        }
        public void ConnectToChildBroker(String newProcessURL) {
            IBrokerRemoteService child = (IBrokerRemoteService)Activator.GetObject(
                typeof(IBrokerRemoteService),
                newProcessURL);
            childrenBrokerTable.Add(child.ProcessName, child);
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
            String[] flags = String.Join(" ", args).Split('-'),
                     parent = flags[1].Split(' '),
                     children = flags[2].Split(' ');

            broker.Connect();
            Console.WriteLine(args[3]);
            broker.ConnectToPuppetMaster(args[3]);
            if (parent.Length > 1) {
                broker.ConnectToParentBroker(parent[1]);
            }
            if (children.Length > 1) {
                for(int index = 2; index < children.Length; index++) {
                    broker.ConnectToChildBroker(children[index]);
                }
            }
            broker.Debug();

            Console.ReadLine();
        }
    }
}