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
        private IConversation conversation;

        public Form1() {
            InitializeComponent();
        }

        private void connectButton_Click(object sender, EventArgs e) {
            conversation = ChatClient.Connect(nicknameBox.Text, portBox.Text);
            connectButton.Text = "Disconnect";
        }

        private void sendButton_Click(object sender, EventArgs e) {
            if (conversation == null) {
                System.Windows.Forms.MessageBox.Show("You must be connected to send messages");
            }
            else {
                chatBox.Text += messageBox.Text + "\n";
                ChatClient.SendMessage(conversation, messageBox.Text);
                messageBox.Text = "";
            }
        }

        private void portBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Enter) {
                connectButton_Click(sender, e);
            }
        }

    }
}
