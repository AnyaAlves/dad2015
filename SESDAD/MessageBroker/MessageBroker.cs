using System;
using System.Collections.Generic;
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

        public MessageBroker(ProcessHeader processHeader) :
            base(processHeader) {
                childBrokerList = new Dictionary<ProcessHeader, IMessageBrokerService>();
                subscriberList = new Dictionary<ProcessHeader, ISubscriberService>();
                topicRoot =  new Topic("", null);
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
            subscriberList.Add(processHeader, newSubscriber);
        }


        public void AddSubscription(ProcessHeader processHeader, String topicName) {
            Topic topic = topicRoot.GetSubtopic(topicName);
            IMessageBrokerService brokerService;
            if (ParentBroker != null && !topic.HasProcesses()) {
                Console.WriteLine("Subscribing to parent");
                ParentBroker.Subscribe(ProcessHeader, topicName);
            }
            if (childBrokerList.TryGetValue(processHeader, out brokerService)) {
                topic.AddBroker(brokerService);
            }
            else if (subscriberList.ContainsKey(processHeader)) {
                topic.AddSubscriber(processHeader);
            }
        }


        public void RemoveSubscription(ProcessHeader processHeader, String topicName) {
            Topic topic = topicRoot.GetSubtopic(topicName);
            topic.RemoveSubscriber(processHeader);
            //notify parent to update
            Console.WriteLine("Removed subcriber: " + processHeader.ProcessName);
        }
        //public void Ack(ProcessHeader processHeader) { }

        public void ForwardEntry(ProcessHeader processHeader, Entry entry) {
            String topicName = entry.TopicName;
            IList<ProcessHeader> topicSubscribers = topicRoot.GetSubscriberList(topicName);

            foreach (ProcessHeader subscriber in topicSubscribers) {
                subscriberList[subscriber].DeliverEntry(entry);
            }
        }

        public void AddChildBroker(ProcessHeader processHeader) {
            IMessageBrokerService child = (IMessageBrokerService)Activator.GetObject(
                typeof(IMessageBrokerService),
                processHeader.ProcessURL);
            childBrokerList.Add(processHeader, child);
        }

        public override void ConnectToParentBroker(String parentBrokerURL) {
            base.ConnectToParentBroker(parentBrokerURL);
            ParentBroker.RegisterChildBroker(ProcessHeader);
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

            Console.ReadLine();
        }
    }
}