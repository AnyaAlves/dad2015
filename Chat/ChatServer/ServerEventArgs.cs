using System;

namespace ChatServer {
    public class PutMessageEventArgs : EventArgs {
        public String nickname;
        public String message;

        public PutMessageEventArgs(String nicknameValue, String messageValue) {
            nickname = nicknameValue;
            message = messageValue;
        }
    }

    public class RegisterClientEventArgs : EventArgs {
        public String nickname;
        public int port;

        public RegisterClientEventArgs(String nicknameValue, int portValue) {
            nickname = nicknameValue;
            port = portValue;
        }
    }

    public class UnregisterClientEventArgs : EventArgs {
        public String nickname;

        public UnregisterClientEventArgs(String nicknameValue) {
            nickname = nicknameValue;
        }
    }
}
