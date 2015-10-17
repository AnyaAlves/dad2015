using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TestApp;

namespace SESDAD.MessageBroker {
    class Program {
        static void Main(string[] args) {
            MessageBroker broker0 = new MessageBroker("broker0", TestApp.Program.site0 , "tcp://1.2.3.4:3333/broker");
            broker0.Connect();
            Console.ReadLine();
        }
    }
}
