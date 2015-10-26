using System;

using SESDAD.CommonTypes;

namespace SESDAD.Processes {
    public interface IProcess {
        ProcessHeader ProcessHeader { get; }

        void ConnectToParentBroker(String parentbrokerURL);

        void Freeze();
        void Unfreeze();
        void Crash();
    }
}
