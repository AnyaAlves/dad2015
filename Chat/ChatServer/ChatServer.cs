using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Collections.Generic;
using System.Net.Sockets;
using CommonTypes;

namespace ChatServer {

    class Server {


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main() {
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
        private IDictionary<String, String> registry = new Dictionary<String, String>();

        private String message;
        public String Message {
            get {
                String outputMessage = message;
                message = "";
                return outputMessage;
            }
            set { message = value; }
        }
        public String getPort(String nickname) {
            return registry[nickname];
        }
        public void Register(String nickname, String port) {
            registry.Add(nickname, port);
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
