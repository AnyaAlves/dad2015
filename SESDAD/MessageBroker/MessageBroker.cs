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

namespace SESDAD.MessageBroker {

    public class MessageBroker : Process {
        private IDictionary<String, IDictionary<String,ISubscriberService>> subscriptions;

        public MessageBroker(String processName, Site site, String processURL) :
            base(processName, site, processURL) {
                subscriptions = new Dictionary<String, IDictionary<String,ISubscriberService>>();
        }

        public override void Connect() {
            channel = new TcpChannel(portNumber);
            ChannelServices.RegisterChannel(channel, true);

            BrokerPubService pubService = new BrokerPubService(this);
            /*serviceReference = */
            RemotingServices.Marshal(
                pubService,
                "BrokerPubService",
                typeof(BrokerPubService));

            BrokerSubService subService = new BrokerSubService(this);
            RemotingServices.Marshal(
                subService,
                "BrokerSubService",
                typeof(BrokerSubService));
        }

        public void registerSubscription(String processName, String processURL, String topicName) {
            IDictionary<String,ISubscriberService> subscribers;

            ISubscriberService newSubscriber =
                (ISubscriberService)Activator.GetObject(
                       typeof(ISubscriberService),
                       processURL);

            if (!subscriptions.TryGetValue(topicName, out subscribers)) {
                subscribers = new Dictionary<String,ISubscriberService>();
                subscriptions.Add(topicName, subscribers);
            }
            subscribers.Add(processName, newSubscriber);
        }

        public void removeSubscription(String processName, String processURL, String topicName) {
            IDictionary<String,ISubscriberService> subscribers;

            if (subscriptions.TryGetValue(topicName, out subscribers)) {
                subscribers.Remove(processName);
            }
        }

        public void ForwardEntry(String processName, String processURL, String entry) {
            IDictionary<String,ISubscriberService> subscribers;
            String topicName = entry;

            if (subscriptions.TryGetValue(topicName, out subscribers)) {
                foreach (KeyValuePair<String,ISubscriberService> subscriber in subscribers) {
                    subscriber.Value.DeliverEntry(entry);
                }
            }
        }
    }
}