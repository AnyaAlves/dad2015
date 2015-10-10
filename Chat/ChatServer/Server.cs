using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Net.Sockets;
using CommonTypes;

namespace ChatServer {
    public class Server {

        private IDictionary<String, String> registry;
        private Queue<KeyValuePair<String, String>> messages;

        public Server() {
            EventHandler<ServerEventArgs> putMessageHandler = new EventHandler<ServerEventArgs>(PutMessage),
                                                registerClientHandler = new EventHandler<ServerEventArgs>(RegisterClient),
                                                unregisterClientHandler = new EventHandler<ServerEventArgs>(UnregisterClient);
            ServerRemoteObject chat = new ServerRemoteObject(putMessageHandler, registerClientHandler, unregisterClientHandler);
            registry = new Dictionary<String, String>();
            messages = new Queue<KeyValuePair<String, String>>();

            TcpChannel channel = new TcpChannel(8086);
            ChannelServices.RegisterChannel(channel, true);
            RemotingServices.Marshal(
                chat,
                "Conversation",
                typeof(ServerRemoteObject));

            Console.WriteLine("Started server");
            Console.Read();
        }


        public void PutMessage(Object sender, ServerEventArgs e) {
            messages.Enqueue(new KeyValuePair<String, String>(e.key, e.value));
            Console.WriteLine("Message: Nickname -> {0} Message -> {1}", e.key, e.value);
        }

        public void RegisterClient(Object sender, ServerEventArgs e) {
            registry.Add(e.key, e.value);
            Console.WriteLine("Registered: Nickname -> {0} Port -> {1}", e.key, e.value);
        }

        public void UnregisterClient(Object sender, ServerEventArgs e) {
            registry.Remove(e.key);
            Console.WriteLine("Removed: Nickname -> {0}", e.key, e.value);
        }

        public String GetPortByNickname(Object sender, ServerEventArgs e) {
            return "batata";//registry[nickname];
        }
    }
}
