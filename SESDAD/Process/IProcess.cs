using System;

using SESDAD.CommonTypes;

namespace SESDAD.Processes {
    public interface IProcess {
        String ProcessName { get; }
        void ConnectToParentBroker(String parentbrokerURL);

        void Freeze();
        void Unfreeze();
        void Crash();
    }
}
