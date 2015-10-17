using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TestApp;

namespace SESDAD.Subscriber {
    class Program {
        static void Main(string[] args) {
            Subscriber subscriber0 = new Subscriber("subscriber0", TestApp.Program.site0 , "tcp://1.2.3.4:3334/sub");
            subscriber0.Connect();
            subscriber0.Subscribe("Cenas Fixes");
            Console.ReadLine();
        }
    }
}
