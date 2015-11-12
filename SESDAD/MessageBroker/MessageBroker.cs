using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.Remoting;

using SESDAD.Commons;


namespace SESDAD.Processes {

    public class MessageBroker : GenericProcess, IMessageBroker {
        private Topic topicRoot;
        // States
        private RoutingPolicyType routingPolicy;
        // Tables
        private IDictionary<ProcessHeader, ISubscriberService> subscriberList;
        private IDictionary<ProcessHeader, IMessageBrokerService> childBrokerList;
        private IDictionary<String, int> brokerSeqNumList;

        EventOrderManager bufferManager;

        public MessageBroker(ProcessHeader processHeader) :
            base(processHeader) {
            topicRoot = new Topic("", null);
            subscriberList = new Dictionary<ProcessHeader, ISubscriberService>();
            childBrokerList = new Dictionary<ProcessHeader, IMessageBrokerService>();
            brokerSeqNumList = new Dictionary<String, int>();
            bufferManager = new EventOrderManager();
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
            //if current broker isn't root and topic is new (hasn't been sent), send to parent 
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
            Event @event;
            if (bufferManager.TryGetPendingEvent(subscriberHeader, publisherHeader, out @event)) {
                SendToSubscriber(subscriberList[subscriberHeader], @event);
            }
        }

        public void SubmitEvent(Event @event) {
            MulticastEvent(new EventContainer(Header, @event, @event.SeqNumber));
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

        public void MulticastEvent(EventContainer eventContainer) {
            Console.WriteLine("New event on buffer:\n" + eventContainer.Event +
                "Multicasted by: " + eventContainer.SenderBroker.ProcessName + " NewSeq: " + eventContainer.NewSeqNumber + "\n");
            InsertIntoBuffer(eventContainer);

            //if current broker isn't root AND parent isn't sender, send to parent
            if (ParentBroker != null && !ParentBroker.Header.Equals(eventContainer.SenderBroker)) {
                SendToBroker(ParentBroker, eventContainer);
            }
        }

        public void ForwardToSubscribers(Event @event) {
            lock (@event) {
                IList<ProcessHeader> topicSubscriberList = topicRoot.GetSubscriberList(@event.TopicName);

                foreach (ProcessHeader subscriber in topicSubscriberList.ToList()) {
                    bufferManager.SetPendingEvent(subscriber, @event);
                    SendToSubscriber(subscriberList[subscriber], @event);
                }
            }
        }


        public void ForwardToBrokers(EventContainer eventContainer) {
            IList<ProcessHeader> brokerList = null;

            if (routingPolicy == RoutingPolicyType.FLOOD) {
                brokerList = childBrokerList.Keys.ToList();
            }
            else if (routingPolicy == RoutingPolicyType.FILTER) {
                brokerList = topicRoot.GetBrokerList(eventContainer.Event.TopicName);
            }
            brokerList.Remove(eventContainer.SenderBroker);

            //send to brokerlist
            lock (childBrokerList) {
                foreach (ProcessHeader childBroker in brokerList) {
                    //FIFO
                    String key = childBroker + eventContainer.Event.PublisherHeader;
                    if (!brokerSeqNumList.ContainsKey(key)) {
                        brokerSeqNumList.Add(key, 0);
                    }
                    eventContainer.NewSeqNumber = brokerSeqNumList[key]++;
                    //NO ORDER+FIFO
                    SendToBroker(childBrokerList[childBroker], eventContainer);
                }
            }        
        }


        //async stuff:
        private void InsertIntoBuffer(EventContainer eventContainer) {
            Action<EventContainer> method = bufferManager.InsertIntoInputBuffer;
            method.BeginInvoke(eventContainer, DoneInsertIntoBuffer, method);
        }
        private void DoneInsertIntoBuffer(IAsyncResult result) {
            lock(this) {
            EventContainer eventContainer = bufferManager.GetNextEvent();
            ForwardToBrokers(eventContainer);
            ForwardToSubscribers(eventContainer.Event);
            }

            var target = (Action<EventContainer>)result.AsyncState;
            target.EndInvoke(result);
        }
        private void FW2Bro(EventContainer eventContainer) {
            Action<EventContainer> method = ForwardToBrokers;
            method.BeginInvoke(eventContainer, DoneFW2Bro, method);
        }
        private void DoneFW2Bro(IAsyncResult result) {
            var target = (Action<EventContainer>)result.AsyncState;
            target.EndInvoke(result);
        }
        private void FW2Sub(Event @event) {
            Action<Event> method = ForwardToSubscribers;
            method.BeginInvoke(@event, DoneFW2Sub, method);
        }
        private void DoneFW2Sub(IAsyncResult result) {
            var target = (Action<Event>)result.AsyncState;
            target.EndInvoke(result);
        }


        private void SendToBroker(IMessageBrokerService broker, EventContainer eventContainer) {
            eventContainer.SenderBroker = Header;
            Action<EventContainer> method = broker.MulticastEvent;
            method.BeginInvoke(eventContainer, DoneSendToBroker, method);
            //broker.MulticastEvent(eventContainer);
        }
        private void DoneSendToBroker(IAsyncResult result) {
            var target = (Action<EventContainer>)result.AsyncState;
            target.EndInvoke(result);
        }


        private void SendToSubscriber(ISubscriberService subscriber, Event @event) {
            Action<Event> method = subscriber.DeliverEvent;
            //method.BeginInvoke(@event, DoneSendToSendToSubscriber, method);
            subscriber.DeliverEvent(@event);
        }
        private void DoneSendToSendToSubscriber(IAsyncResult result) {
            var target = (Action<Event>)result.AsyncState;
            target.EndInvoke(result);
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