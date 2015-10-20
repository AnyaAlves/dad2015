using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SESDAD.PuppetMaster {
    //<summary>
    // Type of Routing Policy
    //</summary>
    public enum RoutingPolicyType {
        FLOODING,
        filter
    };

    //<summary>
    // Type of Ordering
    //</summary>
    public enum OrderingType {
        NO,
        FIFO,
        TOTAL
    };

    //<summary>
    // Type of Logging Level
    //</summary>
    public enum LoggingLevelType {
        full,
        LIGHT
    };
}
