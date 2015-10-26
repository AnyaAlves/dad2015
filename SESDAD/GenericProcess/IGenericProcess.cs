using System;

using SESDAD.CommonTypes;

namespace SESDAD.Processes {
    public interface IGenericProcess {
        ProcessHeader ProcessHeader { get; }

        void ConnectToParentBroker(String parentbrokerURL);

        void Freeze();
        void Unfreeze();
        void Crash();
    }
}
