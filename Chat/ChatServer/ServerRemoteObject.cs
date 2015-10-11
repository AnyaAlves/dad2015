using System;
using CommonTypes;

namespace ChatServer {
    //<summary>
    // Message repository between clients and servers
    //</summary>
    public class ServerRemoteObject : MarshalByRefObject, IServerRemoteObject {
        EventHandler<PutMessageEventArgs> putMessageHandler;
        EventHandler<RegisterClientEventArgs> registerClientHandler;
        EventHandler<UnregisterClientEventArgs> unregisterClientHandler;

        public ServerRemoteObject(
            EventHandler<PutMessageEventArgs> putMessageHandlerValue,
            EventHandler<RegisterClientEventArgs> registerClientHandlerValue,
            EventHandler<UnregisterClientEventArgs> unregisterClientHandlerValue) {
            putMessageHandler = putMessageHandlerValue;
            registerClientHandler = registerClientHandlerValue;
            unregisterClientHandler = unregisterClientHandlerValue;
        }

        public void PutMessage(String nickname, String message) {
            PutMessageEventArgs e = new PutMessageEventArgs(nickname, message);
            if (putMessageHandler != null) {
                putMessageHandler(this, e);
            }
        }

        public void RegisterClient(String nickname, int port) {
            RegisterClientEventArgs e = new RegisterClientEventArgs(nickname, port);
            if (registerClientHandler != null) {
                registerClientHandler(this, e);
            }
        }

        public void UnregisterClient(String nickname) {
            UnregisterClientEventArgs e = new UnregisterClientEventArgs(nickname);
            if (unregisterClientHandler != null) {
                unregisterClientHandler(this, e);
            }
        }
    }
}
