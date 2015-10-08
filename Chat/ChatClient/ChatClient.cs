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
    class Client {
        private String nickname;
        private IConversation conversation;
        private TcpChannel channel;

        public void Connect(String nickname, String port) {
            //validate input
            channel = new TcpChannel();
            ChannelServices.RegisterChannel(channel, true);
            conversation = (IConversation)Activator.GetObject(
                typeof(IConversation),
                "tcp://localhost:8086/Conversation");
            try {
                conversation.RegisterClient(nickname, port);
                this.nickname = nickname;
            }
            catch (SocketException) {
                System.Windows.Forms.MessageBox.Show("Could not locate server");
            }
        }

        public void Disconnect() {
            conversation.UnregisterClient(nickname);
            conversation = null;
            ChannelServices.UnregisterChannel(channel);
        }

        public void SendMessage(String message) {
            try {
                conversation.PutMessage(nickname, message);
            }
            catch (SocketException) {
                System.Windows.Forms.MessageBox.Show("Could not locate server");
            }
        }
    }


    static class App {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
