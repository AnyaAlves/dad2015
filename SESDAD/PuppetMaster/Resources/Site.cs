using System;
using System.Collections.Generic;

namespace SESDAD.Managing {

    internal class Site : Node<Site> {
        private readonly String siteName;
        private String brokerURL;
        private IList<String> pendingURL;

        internal Site(String newSiteName, Site newParent) :
            base(newParent) {
            siteName = newSiteName;
            brokerURL = null;
            pendingURL = new List<String>();
        }

        internal String SiteName {
            get { return siteName; }
        }

        internal String ParentBrokerURL {
            get {
                if (Parent == null) {
                    return null;
                }
                return Parent.BrokerURL;
            }
        }

        internal String BrokerURL {
            get { return brokerURL; }
            set { brokerURL = value; }
        }
    }
}
