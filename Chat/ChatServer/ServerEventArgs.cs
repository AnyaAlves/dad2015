using System;

namespace ChatServer {
    public class ServerEventArgs : EventArgs {
        public bool isValid;
        public String key;
        public String value;

        public ServerEventArgs(String keyValue, String valueValue) {
            isValid = false;
            key = keyValue;
            value = valueValue;
        }

        public ServerEventArgs(String keyValue) {
            new ServerEventArgs(keyValue, null);
        }
    }
}
