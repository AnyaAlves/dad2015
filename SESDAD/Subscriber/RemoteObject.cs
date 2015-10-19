using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SESDAD.CommonTypes;

namespace SESDAD.Subscriber {

    public class SubscriberRemoteObject : MarshalByRefObject, ISubscriberRemoteObject {

        public SubscriberRemoteObject() : base() {}
        
        public void DeliverEntry(String entry) {
            Console.WriteLine("New entry: " + entry);
        }
    }
}
