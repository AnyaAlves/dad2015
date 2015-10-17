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
            System.Diagnostics.Process.Start("SESDAD.PuppetMaster.exe", "Site0 1000 sesdadrc");
            System.Diagnostics.Process.Start("SESDAD.PuppetMaster.exe", "Site1 1001");
        }
    }
}
