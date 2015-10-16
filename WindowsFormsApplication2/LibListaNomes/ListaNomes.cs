using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication2
{
    public class ListaNomes : IListaNomes
    {
        private IList<string> list;

        public ListaNomes() {
            list = new List<string>();
        }

        public void AddName(string name) {
            list.Add(name);
        }
        public string ListNames() {
            string listString = "";
            foreach (string name in list) {
                listString += name + System.Environment.NewLine;
            }
            return listString;
        }
        public void ClearList()
        {
            list.Clear();
        }
    }
}
