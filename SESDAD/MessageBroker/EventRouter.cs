using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SESDAD.Processes {
    public class EventRouter {
   /*     private RoutingPolicyType policyType;
        private IList<String> topicList;
        private IBrokerRemoteService parent;
        private IList<IBrokerRemoteService> children;

        public EventRouter() {
            policyType = RoutingPolicyType.FLOODING;
        }

        public void ChangePolicyToFlooding() {
            policyType = RoutingPolicyType.FLOODING;
        }

        public void ChangePolicyToFiltering() {
            policyType = RoutingPolicyType.FILTERING;
        }

        public void BroadcastEntry(ProcessHeader processHeader, String entry) {
            if (policyType == RoutingPolicyType.FLOODING) { BroadcastByFlooding(); }
            else if (policyType == RoutingPolicyType.FILTERING) { BroadcastByFiltering(); }
        }

        public void BroadcastByFlooding() {
            //send to parent, except if it's null
            //send to each child
            //do not send to sender
        }

        public void BroadcastByFiltering() {
            //send to parent, except if it's null
            //for each child: if topic is in topicList, send it
            //do not send to sender
        }

        public void UpdateTopicList() {
            //parent.updatetopicList()
        }*/
    }
}
