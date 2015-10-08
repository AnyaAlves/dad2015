using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Net.Sockets;

using CommonTypes;

namespace ChatClient {

    static class ChatClient {

        public static IConversation Connect(String nickname, String port) {
            //validate input
            TcpChannel channel = new TcpChannel();
            ChannelServices.RegisterChannel(channel, true);

            IConversation conversation = (IConversation)Activator.GetObject(
                typeof(IConversation),
                "tcp://localhost:8086/Conversation");

            if (conversation == null) {
                throw new NullReferenceException();
            }
            return conversation;
        }

        public static void Disconnect() {
            
        }

        public static void SendMessage(IConversation conversation, String message) {
            try {
                conversation.Message = message;
            }
            catch (SocketException) {
                System.Windows.Forms.MessageBox.Show("Could not locate server");
            }
        }


        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());



        }
    }
}
