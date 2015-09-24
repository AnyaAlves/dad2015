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
            button1.Click += new EventHandler(this.Button1Click);
            button2.Click += new EventHandler(this.Button2Click);
            button3.Click += new EventHandler(this.Button3Click); 
        }

        public void Button1Click(Object sender, System.EventArgs e)
        {
            Button clickedButton = (Button)sender;
            clickedButton.Text = "clicked!!!";
        }

        public void Button2Click(Object sender, System.EventArgs e)
        {
            Button clickedButton = (Button)sender;
            clickedButton.Text = "clicked!!!";
        }

        public void Button3Click(Object sender, System.EventArgs e)
        {
            Button clickedButton = (Button)sender;
            clickedButton.Text = "clicked!!!";
        }
  
    }
}
