using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TestApp;

namespace SESDAD.Publisher {
    class Program {
        static void Main(string[] args) {
            Publisher publisher0 = new Publisher("publisher0", TestApp.Program.site0 , "tcp://1.2.3.4:3335/pub");
            publisher0.Connect();
            publisher0.Publish("Cenas Fixes", "jdgskuayhcbsiufcglasijk");
            Console.ReadLine();
        }
    }
}
