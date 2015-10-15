using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;

using System.Threading;

using SESDAD.PuppetMaster.CommonTypes;

namespace SESDAD.PuppetMaster.Library {
    //<summary>
    // "Provides a singe console from where it is possible to control experiments"
    //</summary>
    public class PuppetMasterService {
        //private IList<String> subscriberList;
        //private IList<String> publisherList;
        //private IList<String> brokerList;

        //private BinaryTree<String> SESDADBinaryTree;
        //private IDictionary<String, BinaryTreeNode<String>> SESDADTable;

        public PuppetMasterService() {
            //subscriberList = new List<String>();
            //publisherList = new List<String>();
            //brokerList = new List<String>();
            //SESDADBinaryTree = new BinaryTree<String>();

        }

        public void Connect(int port = 8086) {
            EventHandler<SiteEventArgs> RootSiteHandler = new EventHandler<SiteEventArgs>(RootSiteEvent);
            EventHandler<SiteEventArgs> SiteHandler = new EventHandler<SiteEventArgs>(SiteEvent);
            EventHandler<CreateProcessEventArgs> BrokerHandler = new EventHandler<CreateProcessEventArgs>(BrokerEvent);
            EventHandler<CreateProcessEventArgs> PublisherHandler = new EventHandler<CreateProcessEventArgs>(PublisherEvent);
            EventHandler<CreateProcessEventArgs> SubscriberHandler = new EventHandler<CreateProcessEventArgs>(SubscriberEvent);
            EventHandler<RoutingPolicyEventArgs> RoutingPolicyHandler = new EventHandler<RoutingPolicyEventArgs>(RoutingPolicyEvent);
            EventHandler<OrderingEventArgs> OrderingHandler = new EventHandler<OrderingEventArgs>(OrderingEvent);
            EventHandler<SubscribeEventArgs> SubscribeHandler = new EventHandler<SubscribeEventArgs>(SubscribeEvent);
            EventHandler<SubscribeEventArgs> UnsubscribeHandler = new EventHandler<SubscribeEventArgs>(UnsubscribeEvent);
            EventHandler<PublishEventArgs> PublishHandler = new EventHandler<PublishEventArgs>(PublishEvent);
            EventHandler StatusHandler = new EventHandler(StatusEvent);
            EventHandler<ControlProcessEventArgs> CrashHandler = new EventHandler<ControlProcessEventArgs>(CrashEvent);
            EventHandler<ControlProcessEventArgs> FreezeHandler = new EventHandler<ControlProcessEventArgs>(FreezeEvent);
            EventHandler<ControlProcessEventArgs> UnfreezeHandler = new EventHandler<ControlProcessEventArgs>(UnfreezeEvent);
            EventHandler<WaitEventArgs> WaitHandler = new EventHandler<WaitEventArgs>(WaitEvent);
            EventHandler<LoggingLevelEventArgs> LoggingLevelHandler = new EventHandler<LoggingLevelEventArgs>(LoggingLevelEvent);

            PuppetMasterRemoteObject remoteObject = new PuppetMasterRemoteObject(
                SiteHandler,
                BrokerHandler,
                PublisherHandler,
                SubscriberHandler,
                RoutingPolicyHandler,
                OrderingHandler,
                SubscribeHandler,
                UnsubscribeHandler,
                PublishHandler,
                StatusHandler,
                CrashHandler,
                FreezeHandler,
                UnfreezeHandler,
                WaitHandler,
                LoggingLevelHandler);

            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, true);
            RemotingServices.Marshal(
                remoteObject,
                "PuppetMasterURL",
                typeof(PuppetMasterRemoteObject));
        }

        private void RootSiteEvent(Object sender, SiteEventArgs eventArgs) {
            Console.WriteLine("SITE_ROOT");
            //SESDADBinaryTree.Root = new BinaryTreeNode<String>("fields[1]");
            //SESDADTable.Add("fields[1]", SESDADBinaryTree.Root);
        }
        private void SiteEvent(Object sender, SiteEventArgs eventArgs) {
            Console.WriteLine("Site");
            //BinaryTreeNode<String> node;
            //SESDADTable.TryGetValue("fields[3]", out node);
            //if (node != null) {
            //    if (node.Left == null) {
            //        node.Left = new BinaryTreeNode<String>("fields[1]");
            //    }
            //    else {
            //        node.Right = new BinaryTreeNode<String>("fields[1]");
            //    }
            //}
        }
        private void BrokerEvent(Object sender, CreateProcessEventArgs eventArgs) {
            Console.WriteLine("Broker");
        }
        private void PublisherEvent(Object sender, CreateProcessEventArgs eventArgs) {
            Console.WriteLine("Publisher");
        }
        private void SubscriberEvent(Object sender, CreateProcessEventArgs eventArgs) {
            Console.WriteLine("Subscriber");
        }
        private void RoutingPolicyEvent(Object sender, RoutingPolicyEventArgs eventArgs) {
            Console.WriteLine("RoutingPolicy");
        }
        private void OrderingEvent(Object sender, OrderingEventArgs eventArgs) {
            Console.WriteLine("Ordering");
        }
        private void SubscribeEvent(Object sender, SubscribeEventArgs eventArgs) {
            Console.WriteLine("Subscribe");
        }
        private void UnsubscribeEvent(Object sender, SubscribeEventArgs eventArgs) {
            Console.WriteLine("Unsubscribe");
        }
        private void PublishEvent(Object sender, PublishEventArgs eventArgs) {
            Console.WriteLine("Publish");
        }
        private void StatusEvent(Object sender, EventArgs eventArgs = null) {
            Console.WriteLine("Status");
        }
        private void CrashEvent(Object sender, ControlProcessEventArgs eventArgs) {
            Console.WriteLine("Crash");
        }
        private void FreezeEvent(Object sender, ControlProcessEventArgs eventArgs) {
            Console.WriteLine("Freeze");
        }
        private void UnfreezeEvent(Object sender, ControlProcessEventArgs eventArgs) {
            Console.WriteLine("Unfreeze");
        }
        private void WaitEvent(Object sender, WaitEventArgs eventArgs) {
            Console.WriteLine("Wait");
        }
        private void LoggingLevelEvent(Object sender, LoggingLevelEventArgs eventArgs) {
            Console.WriteLine("LoggingLevel");
        }
    }
}
