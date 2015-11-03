using System;
using System.Collections.Generic;

using SESDAD.CommonTypes;

namespace SESDAD.Managing {

    internal class Site : Node<Site> {
        private String brokerURL;

        internal Site(String siteName, Site parent) :
            base(siteName, parent) {
            brokerURL = null;
        }

        internal String ParentBrokerURL {
            get {
                return (Parent == null ? null : Parent.BrokerURL);
            }
        }

        internal String BrokerURL {
            get { return brokerURL; }
            set { brokerURL = value; }
        }
    }
}
