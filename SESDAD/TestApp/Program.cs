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
          /*  System.Diagnostics.Process.Start(@"C:\Users\Ana Beatriz\Documents\Visual Studio 2012\Projects\DAD\SESDAD\MessageBroker\bin\Debug\SESDAD.MessageBroker.exe");
            System.Diagnostics.Process.Start(@"C:\Users\Ana Beatriz\Documents\Visual Studio 2012\Projects\DAD\SESDAD\Subscriber\bin\Debug\SESDAD.Subscriber.exe");
            System.Diagnostics.Process.Start(@"C:\Users\Ana Beatriz\Documents\Visual Studio 2012\Projects\DAD\SESDAD\Publisher\bin\Debug\SESDAD.Publisher.exe");
            /*System.Diagnostics.Process.Start("SESDAD.PuppetMaster.exe", "Site0 1000 sesdadrc");
            System.Diagnostics.Process.Start("SESDAD.PuppetMaster.exe", "Site1 1001");*/
            Console.ReadLine();
        }
    }
}
