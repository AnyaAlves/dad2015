using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SESDAD.CommonTypes {

    public class Site : Node {
        private String siteName;
        private String brokerURL;

        public Site(String siteName, Site parent) :
            base(parent) {
            this.siteName = siteName;

            brokerURL = "tcp://1.2.3.4:3333/broker";
        }

        public String getSiteName() { return siteName; }

        public String BrokerURL {
            get { return brokerURL; }
            set { brokerURL = value; }
        }
    }
}
