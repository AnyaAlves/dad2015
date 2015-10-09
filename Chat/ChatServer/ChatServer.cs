using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Collections.Generic;
using System.Net.Sockets;
using CommonTypes;

namespace ChatServer {

    public class Server {

        private IDictionary<String, String> registry;
        private Queue<KeyValuePair<String, String>> messages;

        public Server() {
            EventHandler<ConversationEventArgs> putMessageHandler = new EventHandler<ConversationEventArgs>(PutMessage),
                                                registerClientHandler = new EventHandler<ConversationEventArgs>(RegisterClient),
                                                unregisterClientHandler = new EventHandler<ConversationEventArgs>(UnregisterClient);
            Conversation chat = new Conversation(putMessageHandler, registerClientHandler, unregisterClientHandler);
            registry = new Dictionary<String, String>();
            messages = new Queue<KeyValuePair<String, String>>();

            TcpChannel channel = new TcpChannel(8086);
            ChannelServices.RegisterChannel(channel, true);
            RemotingServices.Marshal(
                chat,
                "Conversation",
                typeof(Conversation));

            Console.WriteLine("Started server");
            Console.Read();
        }


        public void PutMessage(Object sender, ConversationEventArgs e) {
            messages.Enqueue(new KeyValuePair<String, String>(e.key, e.value));
            Console.WriteLine("Message: Nickname -> {0} Message -> {1}", e.key, e.value);
        }

        public void RegisterClient(Object sender, ConversationEventArgs e) {
            registry.Add(e.key, e.value);
            Console.WriteLine("Registered: Nickname -> {0} Port -> {1}", e.key, e.value);
        }

        public void UnregisterClient(Object sender, ConversationEventArgs e) {
            registry.Remove(e.key);
            Console.WriteLine("Removed: Nickname -> {0}", e.key, e.value);
        }

        public String GetPortByNickname(Object sender, ConversationEventArgs e) {
            return "batata";//registry[nickname];
        }
    }

    public class ConversationEventArgs : EventArgs {
        public bool isValid;
        public String key;
        public String value;

        public ConversationEventArgs(String keyValue) {
            isValid = false;
            key = keyValue;
            value = null;
        }

        public ConversationEventArgs(String keyValue, String valueValue) {
            isValid = false;
            key = keyValue;
            value = valueValue;
        }
    }

    //<summary>
    // Message repository between clients and servers
    //</summary>
    public class Conversation : MarshalByRefObject, IConversation {
        EventHandler<ConversationEventArgs> putMessageHandler, registerClientHandler, unregisterClientHandler;

        public Conversation(
            EventHandler<ConversationEventArgs> putMessageHandlerValue,
            EventHandler<ConversationEventArgs> registerClientHandlerValue,
            EventHandler<ConversationEventArgs> unregisterClientHandlerValue) {
                putMessageHandler = putMessageHandlerValue;
                registerClientHandler = registerClientHandlerValue;
                unregisterClientHandler = unregisterClientHandlerValue;
        }

        public void PutMessage(String nickname, String message) {
            ConversationEventArgs e = new ConversationEventArgs(nickname, message);
            if (putMessageHandler != null) {
                putMessageHandler(this,e);
            }
        }

        public void RegisterClient(String nickname, String port) {
            ConversationEventArgs e = new ConversationEventArgs(nickname, port);
            if (registerClientHandler != null) {
                registerClientHandler(this, e);
            }
        }

        public void UnregisterClient(String nickname) {
            ConversationEventArgs e = new ConversationEventArgs(nickname);
            if (unregisterClientHandler != null) {
                unregisterClientHandler(this, e);
            }
        }
    }

    public class App {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static void Main() {
            Server server = new Server();
        }
    }
}
