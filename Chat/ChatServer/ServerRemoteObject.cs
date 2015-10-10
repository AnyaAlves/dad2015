using System;
using CommonTypes;

namespace ChatServer {
    //<summary>
    // Message repository between clients and servers
    //</summary>
    public class ServerRemoteObject : MarshalByRefObject, IServerRemoteObject {
        EventHandler<ServerEventArgs> putMessageHandler, registerClientHandler, unregisterClientHandler;

        public ServerRemoteObject(
            EventHandler<ServerEventArgs> putMessageHandlerValue,
            EventHandler<ServerEventArgs> registerClientHandlerValue,
            EventHandler<ServerEventArgs> unregisterClientHandlerValue) {
            putMessageHandler = putMessageHandlerValue;
            registerClientHandler = registerClientHandlerValue;
            unregisterClientHandler = unregisterClientHandlerValue;
        }

        public void PutMessage(String nickname, String message) {
            ServerEventArgs e = new ServerEventArgs(nickname, message);
            if (putMessageHandler != null) {
                putMessageHandler(this, e);
            }
        }

        public void RegisterClient(String nickname, String port) {
            ServerEventArgs e = new ServerEventArgs(nickname, port);
            if (registerClientHandler != null) {
                registerClientHandler(this, e);
            }
        }

        public void UnregisterClient(String nickname) {
            ServerEventArgs e = new ServerEventArgs(nickname);
            if (unregisterClientHandler != null) {
                unregisterClientHandler(this, e);
            }
        }
    }
}
