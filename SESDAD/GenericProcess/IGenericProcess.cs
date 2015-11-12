using System;

using SESDAD.Commons;

namespace SESDAD.Processes {
    public interface IGenericProcess {
        ProcessHeader Header { get; }

        void ConnectToParentBroker(String parentbrokerURL);

        void Freeze();
        void Unfreeze();
        void Crash();

        String ToString();
    }
}
