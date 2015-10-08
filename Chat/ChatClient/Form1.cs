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

    public partial class Form1 : Form {
        private Client client;

        public Form1() {
            InitializeComponent();
            client = new Client();
        }

        private void connectButton_Click(object sender, EventArgs e) {
            if (connectButton.Text.Equals("Connect")) {
                    client.Connect(nicknameBox.Text, portBox.Text);
                    connectButton.Text = "Disconnect";
                    sendButton.Enabled = true;
                    nicknameBox.Enabled = false;
                    portBox.Enabled = false;
            }
            else {
                client.Disconnect();
                connectButton.Text = "Connect";
                sendButton.Enabled = false;
                nicknameBox.Enabled = true;
                portBox.Enabled = true;
            }
        }

        private void sendButton_Click(object sender, EventArgs e) {
                chatBox.Text += messageBox.Text + "\n";
                client.SendMessage(messageBox.Text);
                messageBox.Text = "";
        }

        private void portBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                connectButton_Click(sender, e);
            }
        }

    }
}
