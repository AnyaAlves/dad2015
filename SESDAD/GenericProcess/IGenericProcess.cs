using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using SESDAD.Commons;

namespace SESDAD.Processes {
    public interface IGenericProcess {
        void Freeze(Task task);
        bool Frozen { get; }

        ProcessHeader Header { get; }

        void ConnectToParentBroker(String parentbrokerURL);

        void Freeze();
        void Unfreeze();
        void Crash();

        String ToString();
    }
}
