using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;

namespace PeopleDataBaseLibrary
{
    public class PeopleDataBase
    {
        public class clsPerson {
            public string FirstName;
        }

        IDictionary<String, int> dataBase = new Dictionary<String, int>;
        int key = 0;

        public void addPerson(String name) {
            clsPerson p = new clsPerson();
            p.FirstName = name;
            dataBase.Add(name, key);
            TextWriter tw = new StreamWriter(key.ToString() + ".txt"); //o que é o arroba?
            XmlSerializer x = new XmlSerializer(p.GetType());
            x.Serialize(tw, p);
            Console.WriteLine(name + " written to file " + key.ToString() + ".txt");
            Console.ReadLine();
            tw.Close();
            key++;
        }

        public TextReader receivePerson(String name) {
            int key;
            if(dataBase.TryGetValue(name, out key)){
                return new StreamReader(@"obj.txt");
            }
            return null;
        }

        public clsPerson convertTextReaderToPerson(TextReader personTextReader) {
            XmlSerializer x = new XmlSerializer(typeof(clsPerson));
            return (clsPerson)x.Deserialize(personTextReader);
        }
    }
}
