using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication2
{
    public partial class Form1 : Form
    {
        public IListaNomes list;

        public Form1()
        {
            list = new ListaNomes();

            InitializeComponent();
            Load += Form1_Load;
        }
        private void Form1_Load(Object sender, EventArgs e)
        {
            button1.MouseClick += new MouseEventHandler(this.Button1Click);
            button3.MouseClick += new MouseEventHandler(this.Button3Click);
            button1.MouseMove += new MouseEventHandler(this.ButtonMouseMove);
            btMostrar.MouseMove += new MouseEventHandler(this.ButtonMouseMove);
            button3.MouseMove += new MouseEventHandler(this.ButtonMouseMove);
            button1.MouseLeave += new EventHandler(this.ButtonMouseLeave);
            btMostrar.MouseLeave += new EventHandler(this.ButtonMouseLeave);
            button3.MouseLeave += new EventHandler(this.ButtonMouseLeave);
        }

        public void Button1Click(Object sender, System.EventArgs e)
        {
            if (textBox1.Text != "")
            {
                list.AddName(textBox1.Text);
                textBox1.Text = "";
            }
        }

        public void Button2Click(Object sender, System.EventArgs e)
        {
            richTextBox1.Text = list.ListNames();
        }

        public void Button3Click(Object sender, System.EventArgs e)
        {
            list.ClearList();
        }

        public void ButtonMouseMove(Object sender, System.EventArgs e)
        {
            Button button = (Button)sender;
            button.BackColor = Color.FromName("ControlDark");
        }

        public void ButtonMouseLeave(Object sender, System.EventArgs e)
        {
            Button button = (Button)sender;
            button.BackColor = Color.FromName("Control");
        }

    }
}
