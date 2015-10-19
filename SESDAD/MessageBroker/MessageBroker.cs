using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Net.Sockets;
using System.Threading;

using SESDAD.CommonTypes;

using TestApp;


namespace SESDAD.MessageBroker {
    using SubscriberList = Dictionary<String,ISubscriberRemoteObject>; //typedef

    public class MessageBroker {
        //process attributes
        private String processName;
        private String siteName;
        private String processURL;
        private int portNumber;
        //service attributes
        private ObjRef serviceReference;
        private TcpChannel channel;
        //broker attributes
        private String parentURL;
        private IList<String> childrenURL;
        private IDictionary<String,SubscriberList> subscriptions;

        public MessageBroker(String processName, String siteName, String processURL, String parentURL) {
            //setting attributes
            subscriptions = new Dictionary<String,SubscriberList>();
            this.processName = processName;
            this.siteName = siteName;
            this.processURL = processURL;
            this.parentURL = parentURL;

            //connect to parent broker
           /* if (parentURL != null) {
                IBrokerRemoteService parentBroker = (IBrokerRemoteService)Activator.GetObject(
                    typeof(IBrokerRemoteService),
                    parentURL);
                parentBroker.RegisterBroker(processName, processURL);
            }*/

            //extracting the port number
            String URLpattern = @"tcp://[\w\.]+:(\d\d\d\d)/\w+";
            Match match = Regex.Match(processURL, URLpattern);
            Int32.TryParse(match.Groups[1].Value, out portNumber);

            //establish connection
            channel = new TcpChannel(portNumber);
            ChannelServices.RegisterChannel(channel, true);

            //make remote service available
            BrokerRemoteService service = new BrokerRemoteService(this);
            RemotingServices.Marshal(
                service,
                "broker",
                typeof(BrokerRemoteService));
        }

        //public void Init() {


        public void registerSubscription(String processName, String processURL, String topicName) {
            SubscriberList subscriberList;

            //get subscriber remote object
            ISubscriberRemoteObject newSubscriber =
                (ISubscriberRemoteObject)Activator.GetObject(
                       typeof(ISubscriberRemoteObject),
                       processURL);
            //if there are no subs to that topic, create a new list of subscriberList
            if (!subscriptions.TryGetValue(topicName, out subscriberList)) {
                subscriberList = new SubscriberList();
                subscriptions.Add(topicName, subscriberList);
            }
            //add the new subscriber
            subscriberList.Add(processName, newSubscriber);
            Console.WriteLine("New subcriber: " + processName);
        }

        public void removeSubscription(String processName, String processURL, String topicName) {
            SubscriberList subscriberList;
            //get subscriber by name and remove it from the list
            if (subscriptions.TryGetValue(topicName, out subscriberList)) {
                subscriberList.Remove(processName);
            }
            Console.WriteLine("Removed subcriber: " + processName);
        }

        public void ForwardEntry(String processName, String processURL, String entry) {
            SubscriberList subscriberList;
            String topicName = entry;

            if (subscriptions.TryGetValue(topicName, out subscriberList)) {
                foreach (KeyValuePair<String,ISubscriberRemoteObject> subscriber in subscriberList) {
                    subscriber.Value.DeliverEntry(entry);
                }
            }

            Console.WriteLine("Forwarding entry to all subscribers");
        }

        public void AddChild(String processName, String processURL) {
            
        }

        public void NotifyParent() { }
    }

    class Program {
        static void Main(string[] args) {
            MessageBroker broker = new MessageBroker("broker0", "site0", "tcp://localhost:8080/broker", "tcp://localhost:8083/broker");
            Console.WriteLine("Hello I'm a MessageBroker");
            Console.ReadLine();
        }
    }
}