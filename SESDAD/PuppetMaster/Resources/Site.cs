using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SESDAD.PuppetMaster {

    public class Site : Node {
        private String siteName;
        private String brokerURL;

        public Site(String siteName, Site parent) :
            base(parent) {
            this.siteName = siteName;
        }

        public String getSiteName() { return siteName; }

        public String getParentBrokerURL() { return ((Site)parent).BrokerURL; }

        public String BrokerURL {
            get { return brokerURL; }
            set { brokerURL = value; }
        }
    }
}
