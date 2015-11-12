using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SESDAD.Commons {
    public interface IPuppetMasterService {
        void WriteIntoLog(String logMessage);
        void WriteIntoFullLog(String logMessage);
    }
}