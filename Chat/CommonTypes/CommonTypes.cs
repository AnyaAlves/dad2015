using System;
using System.Collections.Generic;

namespace CommonTypes {
    //<summary>
    // Message repository Interface
    //</summary>
    public interface IServerRemoteObject {
        void PutMessage(String nickname, String message);
        void RegisterClient(String nickname, int port);
        void UnregisterClient(String nickname);
    }

    public interface IClientRemoteObject {
        void UpdateChat(String lattestMessage);
    }
}