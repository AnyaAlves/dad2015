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
    public partial class Form1 : Form {
        private Client client;

        public Form1() {
            InitializeComponent();
            Action<String> warningLabel_printDelegate = new Action<string>(warningLabel_print),
                           chatBox_printDelegate = new Action<string>(chatBox_print),
                           messageWarningLabel_printDelegate = new Action<string>(messageWarningLabel_print);
            client = new Client(warningLabel_printDelegate, chatBox_printDelegate, messageWarningLabel_printDelegate);
        }

        private void connectButton_Click(object sender, EventArgs e) {
            connectButton.Enabled = false;
            if (connectButton.Text.Equals("Connect")) {
                if(!client.Connect(nicknameBox.Text, portBox.Text)) {
                    nicknameBox.Text = "";
                    portBox.Text = "";
                }
                else {
                    connectButton.Text = "Disconnect";
                    sendButton.Enabled = true;
                    nicknameBox.Enabled = false;
                    portBox.Enabled = false;
                    messageBox.ReadOnly = false;
                }
            }
            else {
                client.Disconnect();
                connectButton.Text = "Connect";
                sendButton.Enabled = false;
                nicknameBox.Enabled = true;
                portBox.Enabled = true;
                messageBox.ReadOnly = true;
            }
            connectButton.Enabled = true;
        }

        private void sendButton_Click(object sender, EventArgs e) {
            sendButton.Enabled = false;
            client.SendMessage(messageBox.Text);
            messageBox.Text = "Write a new message...";
            sendButton.Enabled = true;
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
        }

        private void warningLabel_print(String value) {
            // Make sure we're on the UI thread
            if (warningLabel.InvokeRequired == false) {
                warningLabel.Text = value;
            }
            else {
                // Show progress
                Invoke(new Action<String>(warningLabel_print), new object[] { value });
            }
        }

        private void chatBox_print(String value) {
            // Make sure we're on the UI thread
            if (chatBox.InvokeRequired == false) {
                chatBox.Text += value + "\n";
            }
            else {
                // Show progress
                Invoke(new Action<String>(chatBox_print), new object[] { value });
            }
        }

        private void messageWarningLabel_print(String value) {
            // Make sure we're on the UI thread
            if (messageWarningLabel.InvokeRequired == false) {
                messageWarningLabel.Text = value;
            }
            else {
                // Show progress
                Invoke(new Action<String>(messageWarningLabel_print), new object[] { value });
            }
        }
    }
}
