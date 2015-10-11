using System;
using System.Collections.Generic;
using CommonTypes;

namespace ChatClient {
    //<summary>
    // Message repository between clients and servers
    //</summary>
    public class ClientRemoteObject : MarshalByRefObject, IClientRemoteObject {
        EventHandler<UpdateChatEventArgs> updateChatHandler;

        public ClientRemoteObject(EventHandler<UpdateChatEventArgs> updateChatHandlerValue) {
            updateChatHandler = updateChatHandlerValue;
        }

        public void UpdateChat(String lattestMessage) {
            UpdateChatEventArgs e = new UpdateChatEventArgs(lattestMessage);
            if (updateChatHandler != null) {
                updateChatHandler(this, e);
            }
        }
    }
}
