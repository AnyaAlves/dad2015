using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace WindowsFormsApplication2
{
    public class FormManager
    {
        private IListaNomes list;
        private Form1 form;
        private Button addButton;
        private Button listButton;
        private Button clearButton;

        public FormManager()
        {
            list = new ListaNomes();
            form = new Form1();

            addButton = new Button();
            listButton = new Button();
            clearButton = new Button();

            addButton.Text = "Adicionar nome";
            listButton.Text = "Mostrar lista";
            clearButton.Text = "Limpar lista";

            addButton.Location = new Point(10, 10);
            listButton.Location = new Point(20, 10);
            clearButton.Location = new Point(10, 20);

            form.Controls.Add(addButton);
            //form.Controls.Add(listButton);
            //form.Controls.Add(clearButton);
        }

        public void LoadForm(Object sender, System.EventArgs e)
        {
            Application.Run(form);
            addButton.Click += new EventHandler(this.AddButtonClick);
        }


        public void AddButtonClick(Object sender, System.EventArgs e)
        {
            Button clickedButton = (Button)sender;
            clickedButton.Text = "clicked!!!";
        }
    }
}
