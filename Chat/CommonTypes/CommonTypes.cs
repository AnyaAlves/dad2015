using System;

namespace CommonTypes {
    //<summary>
    // Message repository Interface
    //</summary>
    public interface IServerRemoteObject {
        void PutMessage(String nickname, String message);
        void RegisterClient(String nickname, String port);
        void UnregisterClient(String nickname);
    }
}