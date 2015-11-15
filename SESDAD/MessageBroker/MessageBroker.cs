using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Threading;

using SESDAD.Commons;


namespace SESDAD.Processes {

    public class MessageBroker : GenericProcess, IMessageBroker {
        private Topic topicRoot;
        // States
        private RoutingPolicyType routingPolicy;
        // Tables
        private IDictionary<ProcessHeader, ISubscriberService> subscriberList;
        private IDictionary<ProcessHeader, IMessageBrokerService> adjacentBrokerList;
        private IDictionary<String, int> brokerSeqNumList;

        EventOrderManager orderManager;

        Object locker;

        public MessageBroker(ProcessHeader processHeader) :
            base(processHeader) {
            topicRoot = new Topic("", null);
            subscriberList = new Dictionary<ProcessHeader, ISubscriberService>();
            adjacentBrokerList = new Dictionary<ProcessHeader, IMessageBrokerService>();
            brokerSeqNumList = new Dictionary<String, int>();
            orderManager = new EventOrderManager();
            locker = new Object();
        }

        public RoutingPolicyType RoutingPolicy {
            set { routingPolicy = value; }
        }
        public OrderingType Ordering {
            set { orderManager.Ordering = value; }
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

            if (routingPolicy == RoutingPolicyType.FILTER) {
                SpreadSubscription(Header, topicName);
            }
            topic.AddSubscriber(subscriberHeader);
        }

        public void SpreadSubscription(ProcessHeader brokerHeader, String topicName) {
            Topic topic = topicRoot.GetSubtopic(topicName);
            IList<ProcessHeader> brokerList = adjacentBrokerList.Keys.ToList();
            //if this isn't the origin of subscription
            if (brokerHeader != Header) {
                //mark broker as interested in topic
                topic.AddBroker(brokerHeader);
                //remove the previous sender from the list
                brokerList.Remove(brokerHeader);
            }

            foreach (ProcessHeader broker in brokerList) {
                //if topic is new (hasn't been sent) send it
                if (!topic.AlreadySubscribed(broker)) {
                    adjacentBrokerList[broker].SpreadSubscription(Header, topicName);
                }
            }
        }

        public void RemoveSubscription(ProcessHeader subscriberHeader, String topicName) {
            Topic topic = topicRoot.GetSubtopic(topicName);
            topic.RemoveSubscriber(subscriberHeader);
            //spread unsub
        }

        public void AckDelivery(ProcessHeader subscriberHeader, ProcessHeader publisherHeader) {
            Event @event;
            lock (locker) {
                if (orderManager.TryGetPendingEvent(subscriberHeader, publisherHeader, out @event)) {
                    Task.Run(() => subscriberList[subscriberHeader].DeliverEvent(@event));
                }
            }
        }

        public void SubmitEvent(Event @event) {
            MulticastEvent(new EventContainer(Header, @event, @event.SeqNumber));
        }

        public void AddBroker(ProcessHeader brokerHeader) {
            IMessageBrokerService brokerService = (IMessageBrokerService)Activator.GetObject(
                typeof(IMessageBrokerService),
                brokerHeader.ProcessURL);
            adjacentBrokerList.Add(brokerHeader, brokerService);
        }

        public override void ConnectToParentBroker(String parentBrokerURL) {
            base.ConnectToParentBroker(parentBrokerURL);
            ParentBroker.RegisterBroker(Header);
            adjacentBrokerList.Add(ParentBroker.Header, ParentBroker);
        }

        public void MulticastEvent(EventContainer eventContainer) {
            Console.WriteLine("New event on buffer:\n" + eventContainer.Event +
                "Multicasted by: " + eventContainer.SenderBroker.ProcessName + " NewSeq: " + eventContainer.NewSeqNumber + "\n");

            Task.Run(() => orderManager.EnqueueEvent(eventContainer));

            Task.Run(() => {
                lock (adjacentBrokerList) {
                    ForwardToBrokers(orderManager.GetNextBrokerEvent());
                }
            });
            Task.Run(() => {
                lock (subscriberList) {
                    ForwardToSubscribers(orderManager.GetNextSubscriberEvent());
                }
            });
        }

        private void ForwardToSubscribers(Event @event) {
            //Console.WriteLine("ForwardToSubscribers Thread: " + Thread.CurrentThread.ManagedThreadId + "\n");
            IList<ProcessHeader> forwardingSubscriberList = topicRoot.GetSubscriberList(@event.TopicName);

            foreach (ProcessHeader subscriber in forwardingSubscriberList.ToList()) {
                if (!orderManager.TrySetPendingEvent(subscriber, @event)) {
                    subscriberList[subscriber].DeliverEvent(@event);
                }
            }
        }

        public void ForwardToBrokers(EventContainer eventContainer) {
            //Console.WriteLine("ForwardToBrokers Thread: " + Thread.CurrentThread.ManagedThreadId + "\n");
            IList<ProcessHeader> forwardingBrokerList = null;
            EventContainer newEventContainer = eventContainer.Clone();

            if (routingPolicy == RoutingPolicyType.FLOOD) {
                forwardingBrokerList = adjacentBrokerList.Keys.ToList();
            }
            else if (routingPolicy == RoutingPolicyType.FILTER) {
                forwardingBrokerList = topicRoot.GetBrokerList(eventContainer.Event.TopicName);
            }
            forwardingBrokerList.Remove(eventContainer.SenderBroker);

            //send to brokerlist
            foreach (ProcessHeader broker in forwardingBrokerList) {
                if (orderManager.Ordering == OrderingType.FIFO) {
                    String key = broker + eventContainer.Event.PublisherHeader;
                    if (!brokerSeqNumList.ContainsKey(key)) {
                        brokerSeqNumList.Add(key, 0);
                    }
                    newEventContainer.NewSeqNumber = brokerSeqNumList[key]++;
                }
                newEventContainer.SenderBroker = Header;
                Task.Run(() => adjacentBrokerList[broker].MulticastEvent(newEventContainer));
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
                " Adjacent Brokers :" + nl +
                String.Join(nl, adjacentBrokerList.Keys) + nl +
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