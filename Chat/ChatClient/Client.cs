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
        private Action<String> printOnWarningLabel, printOnChatBox, printOnMessageWarningLabel;

        public Client(Action<String> printOnWarningLabelValue,
                      Action<String> printOnChatBoxValue,
                      Action<String> printOnMessageWarningLabelValue) {
            MINPORT = 1;
            MAXPORT = 10000;
            printOnWarningLabel = printOnWarningLabelValue;
            printOnChatBox = printOnChatBoxValue;
            printOnMessageWarningLabel = printOnMessageWarningLabelValue;
        }

        public bool Connect(String nickname, String port) {
            if (nickname.Equals("")) {
                printOnWarningLabel("Warning: The client cannot connect to the chat using an empty nickname.");
                return false;
            }
            int portNumber;
            if (!int.TryParse(port, out portNumber) ||
                portNumber < MINPORT ||
                portNumber > MAXPORT) {
                printOnWarningLabel("Warning: The client can only connect through ports between " + MINPORT + " and " + MAXPORT + ".");
                return false;
            }
            EventHandler<UpdateChatEventArgs> updateChatHandler = new EventHandler<UpdateChatEventArgs>(UpdateChat);
            channel = new TcpChannel(portNumber);
            ChannelServices.RegisterChannel(channel, true);
            ClientRemoteObject remoteObject = new ClientRemoteObject(updateChatHandler);
            conversation = (IServerRemoteObject)Activator.GetObject(
                typeof(IServerRemoteObject),
                "tcp://localhost:8086/Conversation");
            RemotingServices.Marshal(
                remoteObject,
                "Conversation",
                typeof(ClientRemoteObject));
            try {
                // Create delegate to remote method
                Action<String, int> RemoteDel = new Action<String, int>(conversation.RegisterClient);
                // Create delegate to local callback
                AsyncCallback RemoteCallback = new AsyncCallback(Client.RegisterClientCallBack);
                // Call remote method
                IAsyncResult RemAr = RemoteDel.BeginInvoke(nickname, portNumber, RemoteCallback, null);
            }
            catch (SocketException) {
                printOnChatBox("Could not locate server");
                return false;
            }

            this.nickname = nickname;
            return true;
        }

        public void Disconnect() {

            // Create delegate to remote method
            Action<String> RemoteDel = new Action<String>(conversation.UnregisterClient);
            // Create delegate to local callback
            AsyncCallback RemoteCallback = new AsyncCallback(Client.UnregisterClientCallBack);
            // Call remote method
            IAsyncResult RemAr = RemoteDel.BeginInvoke(nickname, RemoteCallback, null);

            conversation = null;
            ChannelServices.UnregisterChannel(channel);
        }

        public bool SendMessage(String message) {
            if (String.IsNullOrWhiteSpace(message)) {
                printOnMessageWarningLabel("Warning: The chat cannot send an empty message.");
                return false;
            }
            try {
                // Create delegate to remote method
                Action<String, String> RemoteDel = new Action<String, String>(conversation.PutMessage);
                // Create delegate to local callback
                AsyncCallback RemoteCallback = new AsyncCallback(Client.SendMessageCallBack);
                // Call remote method
                IAsyncResult RemAr = RemoteDel.BeginInvoke(nickname, message, RemoteCallback, null);
            }
            catch (SocketException) {
                printOnChatBox("Could not locate server");
                return false;
            }
            return true;
        }

        // This are the calls that the AsyncCallBack delegates will reference.
        public static void RegisterClientCallBack(IAsyncResult ar) {}
        public static void UnregisterClientCallBack(IAsyncResult ar) {}
        public static void SendMessageCallBack(IAsyncResult ar) {}

        public void UpdateChat(Object sender, UpdateChatEventArgs eventArgs) {
            printOnChatBox(eventArgs.lattestMessage);
        }
    }
}
