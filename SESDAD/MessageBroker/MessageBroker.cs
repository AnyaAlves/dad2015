﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Threading;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Diagnostics;
using System.Text.RegularExpressions;

using SESDAD.Commons;


namespace SESDAD.Processes {

    public class MessageBroker : GenericProcess, IMessageBroker {
        private Topic topicRoot;
        private IMessageBrokerService mainBroker;
        // States
        private RoutingPolicyType routingPolicy;
        // Tables
        private IDictionary<ProcessHeader, ISubscriberService> subscriberList;
        private IDictionary<ProcessHeader, IMessageBrokerService> adjacentBrokerList,
                                                                  replicatedBrokerList;
        private IDictionary<String, int> brokerSeqNumList;
        private int rootSeqNumber;

        EventOrderManager orderManager, secondaryManager;

        Object locker;

        public MessageBroker(ProcessHeader processHeader) :
            base(processHeader) {
            topicRoot = new Topic("", null);
            mainBroker = null;
            subscriberList = new Dictionary<ProcessHeader, ISubscriberService>();
            adjacentBrokerList = new Dictionary<ProcessHeader, IMessageBrokerService>();
            replicatedBrokerList = new Dictionary<ProcessHeader, IMessageBrokerService>();
            brokerSeqNumList = new Dictionary<String, int>();
            orderManager = new EventOrderManager();
            secondaryManager = new EventOrderManager();
            locker = new Object();
            rootSeqNumber = 0;
        }

        public RoutingPolicyType RoutingPolicy {
            set { routingPolicy = value; }
        }
        public OrderingType Ordering {
            set { orderManager.Ordering = value; }
        }
        public IList<ProcessHeader> ReplicatedBrokerList {
            get { return replicatedBrokerList.Keys.ToList(); }
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

            topic.AddSubscriber(subscriberHeader);
            if (routingPolicy == RoutingPolicyType.FILTER) {
                SpreadSubscription(Header, topicName);
            }
        }

        public void SpreadSubscription(ProcessHeader brokerHeader, String topicName) {
            Topic topic = topicRoot.GetSubtopic(topicName);
            IList<ProcessHeader> brokerList = adjacentBrokerList.Keys.ToList();
            //if this isn't the origin of subscription
            if (!brokerHeader.Equals(Header)) {
                //mark broker as interested in topic
                topic.AddBroker(brokerHeader);
                //remove the previous sender from the list
                brokerList.Remove(brokerHeader);
            }

            foreach (ProcessHeader broker in brokerList) {
                try {
                    //send it
                    adjacentBrokerList[broker].SpreadSubscription(Header, topicName);
                } catch (SocketException) {
                    Console.WriteLine("Unregistered " + broker.ProcessName);
                    adjacentBrokerList.Remove(broker);
                }
            }
        }

        public void RemoveSubscription(ProcessHeader subscriberHeader, String topicName) {
            Topic topic = topicRoot.GetSubtopic(topicName);

            topic.RemoveSubscriber(subscriberHeader);
            if (routingPolicy == RoutingPolicyType.FILTER) {
                SpreadUnsubscription(Header, topicName);
            }
        }

        public void SpreadUnsubscription(ProcessHeader brokerHeader, String topicName)
        {
            Topic topic = topicRoot.GetSubtopic(topicName);
            IList<ProcessHeader> brokerList = adjacentBrokerList.Keys.ToList();
            //if this isn't the origin of subscription
            if (!brokerHeader.Equals(Header)) {
                //mark broker as interested in topic
                topic.RemoveBroker(brokerHeader);
                //remove the previous sender from the list
                brokerList.Remove(brokerHeader);
            }

            foreach (ProcessHeader broker in brokerList) {
                try {
                    //send it
                    adjacentBrokerList[broker].SpreadUnsubscription(Header, topicName);
                } catch (SocketException) {
                    Console.WriteLine("Unregistered " + broker.ProcessName);
                    adjacentBrokerList.Remove(broker);
                }
            }
        }

        public void AckDelivery(ProcessHeader subscriberHeader, ProcessHeader publisherHeader) {
            Event @event;
            lock (locker) {
                if (orderManager.TryGetPendingEvent(subscriberHeader, publisherHeader, out @event)) {
                    subscriberList[subscriberHeader].DeliverEvent(@event);
                }
            }
        }

        public void SubmitEvent(Event @event) {
            EventContainer eventContainer = new EventContainer(Header, @event, @event.SeqNumber);

            if (orderManager.Ordering == OrderingType.NO_ORDER || orderManager.Ordering == OrderingType.FIFO) {
                MulticastEvent(eventContainer);
            } else if (orderManager.Ordering == OrderingType.TOTAL_ORDER) {
                UnicastEvent(eventContainer);
            }
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

        public void UnicastEvent(EventContainer eventContainer) {
            //Console.WriteLine("New event on buffer:\n" + eventContainer.Event +
            //    "Unicasted by: " + eventContainer.SenderBroker.ProcessName + " NewSeq: " + eventContainer.NewSeqNumber + "\n");

            Task.Run(() => secondaryManager.EnqueueEventNoOrder(eventContainer));

            Task.Run(() => {
                lock (adjacentBrokerList) {
                    ForwardToParent(secondaryManager.GetNextBrokerEvent());
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
            //EventContainer newEventContainer = eventContainer.Clone();
            EventContainer newEventContainer = new EventContainer(eventContainer);

            if (routingPolicy == RoutingPolicyType.FLOOD) {
                forwardingBrokerList = adjacentBrokerList.Keys.ToList();
            }
            else if (routingPolicy == RoutingPolicyType.FILTER) {
                forwardingBrokerList = topicRoot.GetBrokerList(eventContainer.Event.TopicName);
            }
            forwardingBrokerList.Remove(eventContainer.SenderBroker);
            forwardingBrokerList.Remove(Header);

            //send to brokerlist
            int newSeqNumber = 0;
            foreach (ProcessHeader broker in forwardingBrokerList) {
                if (orderManager.Ordering == OrderingType.FIFO) {
                    String key = broker + eventContainer.Event.PublisherHeader;
                    if (!brokerSeqNumList.ContainsKey(key)) {
                        brokerSeqNumList.Add(key, 0);
                    }
                    //newEventContainer.NewSeqNumber = brokerSeqNumList[key]++;
                    newSeqNumber = brokerSeqNumList[key]++;
                }

                if (broker.ProcessName.Equals("bro4"))
                Console.WriteLine("New event on buffer:\n" + newEventContainer.Event +
"Multicasted to: " + broker.ProcessName + " NewSeq: " + newSeqNumber + "\n");
                
                newEventContainer.SenderBroker = Header;
                Task.Run(() => adjacentBrokerList[broker].MulticastEvent(new EventContainer(Header, eventContainer.Event, newSeqNumber)));
            }
        }

        public void ForwardToParent(EventContainer eventContainer)
        {
            //Console.WriteLine("ForwardToBrokers Thread: " + Thread.CurrentThread.ManagedThreadId + "\n");
            EventContainer newEventContainer = eventContainer.Clone();

            if (ParentBroker != null) {
                Task.Run(() => ParentBroker.UnicastEvent(newEventContainer));
            } else {
                newEventContainer.NewSeqNumber = rootSeqNumber++;
                newEventContainer.SenderBroker = Header;

                Task.Run(() => this.MulticastEvent(newEventContainer));
            }
        }

        public void Replicate(string[] args, int numberOfReplications) {
            Match match;
            int port,
                brokerPort;
            String brokerFile,
                   brokerUrl,
                   arguments;
            IMessageBrokerService broker;
            ProcessHeader brokerHeader;

            // Extract main broker info
            match = Regex.Match(args[2], @"^tcp://[\w\.]+:(\d{4,5})/(\w+)$");
            port = Int32.Parse(match.Groups[1].Value);
            brokerFile = Process.GetCurrentProcess().MainModule.FileName;
            for (int increment = 1; increment <= numberOfReplications; increment++) {
                // Calculate replicated broker port, url and arguments
                brokerPort = port + 4000 * increment;
                brokerUrl = "tcp://localhost:" + brokerPort + "/broker";
                arguments = args[0] + " " + args[1] + " " + brokerUrl;

                // Connect with newly created replicated broker service
                Process.Start(brokerFile, arguments);
                broker = (IMessageBrokerService)Activator.GetObject(
                    typeof(IMessageBrokerService),
                    brokerUrl);
                broker.ConnectToMainBroker(args[2]);
                brokerHeader = broker.Header;

                // Add replicated broker service
                replicatedBrokerList.Add(brokerHeader, broker);
            }
        }

        public void ConnectToMainBroker(String mainBrokerURL) {
            mainBroker = (IMessageBrokerService)Activator.GetObject(
                typeof(IMessageBrokerService),
                mainBrokerURL);
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
            int numberOfReplications = 2;
            ProcessHeader processHeader = new ProcessHeader(args[0], ProcessType.BROKER, args[1], args[2]);
            MessageBroker process = new MessageBroker(processHeader);

            process.LaunchService<MessageBrokerService, IMessageBroker>(((IMessageBroker)process));
            if (args.Length == 4) {
                process.ConnectToParentBroker(args[3]);
                ///process.Replicate(args, numberOfReplications);
            } if (args.Length == 5) {
                //process.Replicate(args, numberOfReplications);
            }


            Console.WriteLine(process);
            Console.ReadLine();
        }
    }
}