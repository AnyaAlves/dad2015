using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Net.Sockets;
using System.Threading;

using CommonTypes;

namespace ChatClient {
    class Client {
        public readonly int MINPORT, MAXPORT;
        private String nickname;
        private IServerRemoteObject conversation;
        private TcpChannel channel;

        public Client() {
            MINPORT = 1;
            MAXPORT = 10000;
        }

        public void Connect(String nickname, String port) {
            //validate input
            channel = new TcpChannel();
            ChannelServices.RegisterChannel(channel, true);
            conversation = (IServerRemoteObject)Activator.GetObject(
                typeof(IServerRemoteObject),
                "tcp://localhost:8086/Conversation");
            try {
                // Create delegate to remote method
                RegisterClientDelegate RemoteDel = new RegisterClientDelegate(conversation.RegisterClient);
                // Create delegate to local callback
                AsyncCallback RemoteCallback = new AsyncCallback(Client.RegisterClientCallBack);
                // Call remote method
                IAsyncResult RemAr = RemoteDel.BeginInvoke(nickname, port, RemoteCallback, null);

                this.nickname = nickname;
            }
            catch (SocketException) {
                System.Windows.Forms.MessageBox.Show("Could not locate server");
            }
        }

        public void Disconnect() {

            // Create delegate to remote method
            UnregisterClientDelegate RemoteDel = new UnregisterClientDelegate(conversation.UnregisterClient);
            // Create delegate to local callback
            AsyncCallback RemoteCallback = new AsyncCallback(Client.UnregisterClientCallBack);
            // Call remote method
            IAsyncResult RemAr = RemoteDel.BeginInvoke(nickname, RemoteCallback, null);

            conversation = null;
            ChannelServices.UnregisterChannel(channel);
        }

        public void SendMessage(String message) {
            try {
                // Create delegate to remote method
                SendMessageDelegate RemoteDel = new SendMessageDelegate(conversation.PutMessage);
                // Create delegate to local callback
                AsyncCallback RemoteCallback = new AsyncCallback(Client.SendMessageCallBack);
                // Call remote method
                IAsyncResult RemAr = RemoteDel.BeginInvoke(nickname, message, RemoteCallback, null);
            }
            catch (SocketException) {
                System.Windows.Forms.MessageBox.Show("Could not locate server");
            }
        }

        // This is the call that the AsyncCallBack delegate will reference.
        public static void RegisterClientCallBack(IAsyncResult ar) {
            // Alternative 2: Use the callback to get the return value
            RegisterClientDelegate del = (RegisterClientDelegate)((AsyncResult)ar).AsyncDelegate;

            return;
        }

        // This is the call that the AsyncCallBack delegate will reference.
        public static void UnregisterClientCallBack(IAsyncResult ar) {
            // Alternative 2: Use the callback to get the return value
            UnregisterClientDelegate del = (UnregisterClientDelegate)((AsyncResult)ar).AsyncDelegate;

            return;
        }

        // This is the call that the AsyncCallBack delegate will reference.
        public static void SendMessageCallBack(IAsyncResult ar) {
            // Alternative 2: Use the callback to get the return value
            SendMessageDelegate del = (SendMessageDelegate)((AsyncResult)ar).AsyncDelegate;

            return;
        }
    }

    public delegate void RegisterClientDelegate(String nickname, String port);
    public delegate void UnregisterClientDelegate(String nickname);
    public delegate void SendMessageDelegate(String nickname, String message);

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
