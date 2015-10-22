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

        internal IList<String> ChildrenBrokerURL {
            get {
                IList<String> childrenList = new List<String>();
                foreach (Site child in children) {
                    if (child.BrokerURL != null) {
                        childrenList.Add(child.BrokerURL);
                    }
                }
                return childrenList;
            }
        }

        internal String BrokerURL {
            get { return brokerURL; }
            set {
                brokerURL = value;
            }
        }

        internal void SaveURL(String savedURL) {
            pendingURL.Add(savedURL);
        }

        internal String GetPendingURL() {
            return String.Join(" ", pendingURL);
        }
    }
}
