using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Collections.Generic;
using System.Net.Sockets;
using CommonTypes;

namespace ChatServer {

    public class App {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static void Main() {
            ConversationHandler conversationHandler = new ConversationHandler(Conversation_Set);
            RegistryHandler registryHandler = new RegistryHandler(Register_Set);
            Conversation chat = new Conversation();

            TcpChannel channel = new TcpChannel(8086);
            ChannelServices.RegisterChannel(channel, true);
            RemotingServices.Marshal(
                chat,
                "Conversation",
                typeof(Conversation));
            Console.WriteLine("Started server");
            Console.Read();
        }
        
        public static void Conversation_Set(object sender, ConversationEventArgs e) {
            Console.WriteLine("Conversation_Set");
        }

        public static void Register_Set(object sender, RegistryEventArgs e) {
            Console.WriteLine("Register_Set");
        }
    }

    public delegate void ConversationHandler(object sender, ConversationEventArgs e);

    public class ConversationEventArgs : EventArgs {
        public String message;
        public bool isValid;

        public ConversationEventArgs(String message) {
            this.message = message;
            this.isValid = false;
        }
    }

    public delegate void RegistryHandler(object sender, RegistryEventArgs e);

    public class RegistryEventArgs : EventArgs {
        public String clientName;
        public bool isValid;

        public RegistryEventArgs(String clientName) {
            this.clientName = clientName;
            this.isValid = false;
        }
    }

    //<summary>
    // Message repository between clients and servers
    //</summary>
    public class Conversation : MarshalByRefObject, IConversation {
        private IDictionary<String,String> registry;
        private Queue<KeyValuePair<String,String>> messages;

        public Conversation() {
            registry = new Dictionary<String,String>();
            messages = new Queue<KeyValuePair<String,String>>();
        }

        public void PutMessage(String nickname, String message) {
            messages.Enqueue(new KeyValuePair<String,String>(nickname, message));
            
        }

        public void RegisterClient(String nickname, String port) {
            registry.Add(nickname, port);
        }

        public void UnregisterClient(String nickname) {
            registry.Remove(nickname);
        }

        public String getPortByNickname(String nickname) {
            return registry[nickname];
        }
        /*   private event ConversationHandler conversationHandler;
           private event RegistryHandler registryHandler;
        
           public Conversation(ConversationHandler conversartionHandler, RegistryHandler registryHandler)
           {
               this.conversationHandler = conversationHandler;
               this.registryHandler = registryHandler;
           }

           public void Message(String value)
           {
               ConversationEventArgs arguments = new ConversationEventArgs(value);
               if (this.conversationHandler != null)
               {
                   this.conversationHandler(this, arguments);
               }
           }

           public void ClientName(String value)
           {
               RegistryEventArgs arguments = new RegistryEventArgs(value);
               if (this.registryHandler != null)
               {
                   this.registryHandler(this, arguments);
               }
           }

           public String ShowMessage()
           {
               return "Teste";
           }
         * */
    }
}
