using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SESDAD.CommonTypes;

namespace SESDAD.Subscriber {

    public class SubscriberRemoteObject : MarshalByRefObject, ISubscriberRemoteService {
        IAdministratorService puppetMaster;

        public SubscriberRemoteObject() : base() {}

        public void ConnectToPuppetMaster(String puppetMasterURL) {
            puppetMaster = (IAdministratorService)Activator.GetObject(
                 typeof(IAdministratorService),
                 puppetMasterURL);
        }

        public void Freeze() { }
        public void Unfreeze() { }
        public void Crash() {
            Environment.Exit(-1);
        }

        public void Subscribe(String topicName) { }
        public void Unsubscribe(String topicName) { }

        public void DeliverEntry(Entry entry) {
            Console.WriteLine("New entry!");
            Console.WriteLine(entry.getTopicName() + ": " + entry.getContent());
            //subscriber.receive
            //puppetMaster.WriteIntoLog
        }
    }
}
