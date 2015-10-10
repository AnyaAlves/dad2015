using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using CommonTypes;

namespace ChatClient {

    delegate void delegateChat(String s);

    enum MessageState {
        empty,
        filled
    }

    public partial class Form1 : Form {
        private Client client;
        private MessageState messageState = MessageState.empty;

        public Form1() {
            InitializeComponent();
            client = new Client();
        }

        private void connectButton_Click(object sender, EventArgs e) {
            if (connectButton.Text.Equals("Connect")) {
                int portNumber;
                if (nicknameBox.Text.Equals("")) {
                    warningLabel.Text = "Warning: The client cannot connect to the chat using an empty nickname.";
                    return;
                }
                if (!int.TryParse(portBox.Text, out portNumber) ||
                    portNumber < client.MINPORT ||
                    portNumber > client.MAXPORT) {
                    warningLabel.Text = "Warning: The client can only connect through ports between " + client.MINPORT + " and " + client.MAXPORT + ".";
                    portBox.Text = "";
                    return;
                }
                client.Connect(nicknameBox.Text, portBox.Text);
                connectButton.Text = "Disconnect";
                sendButton.Enabled = true;
                nicknameBox.Enabled = false;
                portBox.Enabled = false;
                messageBox.ReadOnly = false;
            }
            else {
                client.Disconnect();
                connectButton.Text = "Connect";
                sendButton.Enabled = false;
                nicknameBox.Enabled = true;
                portBox.Enabled = true;
                messageBox.ReadOnly = true;
            }
        }

        private void sendButton_Click(object sender, EventArgs e) {
            if (messageState == MessageState.empty) {
                messageWarningLabel.Text = "Warning: The chat cannot send an empty message.";
                return;
            }
            chatBox.Text += messageBox.Text + "\n";
            client.SendMessage(messageBox.Text);
            messageBox.Text = "Write a new message...";
            messageState = MessageState.empty;
        }

        private void portBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                connectButton_Click(sender, e);
            }
        }

        private void nicknameBox_Enter(object sender, EventArgs e) {
            warningLabel.Text = "";
        }

        private void portBox_Enter(object sender, EventArgs e) {
            warningLabel.Text = "";
        }

        private void messageBox_Enter(object sender, EventArgs e) {
            messageWarningLabel.Text = "";
            messageBox.Text = "";
        }

        private void messageBox_Leave(object sender, EventArgs e) {
            if (messageBox.Text.Equals("")) {
                messageBox.Text = "Write a new message...";
            }
            else {
                messageState = MessageState.filled;
            }
        }
    }
}
