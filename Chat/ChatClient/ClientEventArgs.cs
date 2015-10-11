using System;
using System.Collections.Generic;
using CommonTypes;

namespace ChatClient {
    public class UpdateChatEventArgs : EventArgs {
        public String lattestMessage;

        public UpdateChatEventArgs(String lattestMessageValue) {
            lattestMessage = lattestMessageValue;
        }
    }
}
