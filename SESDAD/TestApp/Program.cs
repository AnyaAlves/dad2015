using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SESDAD.CommonTypes;

namespace TestApp {
    public class Program {
        public static Site site0 = new Site("site0", null); 

        static void Main(string[] args) {
        /*    System.Diagnostics.Process.Start(@"C:\Users\Ana Beatriz\Documents\Visual Studio 2012\Projects\DAD\SESDAD\MessageBroker\bin\Debug\SESDAD.MessageBroker.exe");
            System.Diagnostics.Process.Start(@"C:\Users\Ana Beatriz\Documents\Visual Studio 2012\Projects\DAD\SESDAD\Publisher\bin\Debug\SESDAD.Publisher.exe");
            System.Diagnostics.Process.Start(@"C:\Users\Ana Beatriz\Documents\Visual Studio 2012\Projects\DAD\SESDAD\Subscriber\bin\Debug\SESDAD.Subscriber.exe");
        /*    IList<Process> processes = new List<Process>();
            Site site0 = new Site("site0", null);
            MessageBroker broker0 = new MessageBroker("broker0", site0, "tcp://1.2.3.4:3333/broker");
            Publisher publisher0 = new Publisher("publisher0", site0, "tcp://1.2.3.4:3334/pub");
            Subscriber subscriber0 = new Subscriber("subscriber0", site0, "tcp://1.2.3.4:3335/sub");
            processes.Add(broker0); processes.Add(publisher0); processes.Add(subscriber0);
            foreach (Process process in processes) {
                process.Connect();
            }

            subscriber0.Subscribe("Cenas Fixes");
            publisher0.Publish("Cenas Fixes", "jdgskuayhcbsiufcglasijk"); */
            Console.ReadLine();
        }
    }
}
