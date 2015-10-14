using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PuppetMasterCommonTypes;

namespace PuppetMasterService {
    class PuppetMasterRemoteObject : MarshalByRefObject, IExecutioner {
        public String ExecuteCommand(String command) {
            return "";
        }
    }
}
