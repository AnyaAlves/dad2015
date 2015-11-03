using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SESDAD.CommonTypes {
    /// <summary>
    ///  Type of Routing Policy
    /// </summary>
    public enum RoutingPolicyType {
        FLOODING,
        FILTERING
    };
    /// <summary>
    ///  Type of Ordering
    /// </summary>
    public enum OrderingType {
        NO_ORDER,
        FIFO,
        TOTAL_ORDER
    };
    /// <summary>
    ///  Type of Process
    /// </summary>
    public enum ProcessType {
        BROKER,
        PUBLISHER,
        SUBSCRIBER
    }
}
