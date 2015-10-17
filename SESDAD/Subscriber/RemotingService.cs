using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SESDAD.CommonTypes;

namespace SESDAD.Subscriber {

    public class SubscriberService : MarshalByRefObject, ISubscriberService {

        public SubscriberService() : base() {}
        
        public void DeliverEntry(String entry) {
            Console.WriteLine("New entry: " + entry);
        }
    }
}
