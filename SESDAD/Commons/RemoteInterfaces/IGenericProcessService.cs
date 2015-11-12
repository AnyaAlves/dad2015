using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SESDAD.Commons {
    public interface IGenericProcessService {
        ProcessHeader Header { get; }

        void ConnectToPuppetMaster(String puppetMasterURL);
        void ConnectToParentBroker(String parentbrokerURL);

        void ForceFreeze();
        void ForceUnfreeze();
        void ForceCrash();

        String GetStatus();
    }
}