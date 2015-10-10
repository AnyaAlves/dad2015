using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster {
    class Program {
        //<summary>
        // Entry Point
        //</summary>
        static void Main(string[] args) {
            PuppetMaster puppetMaster = new PuppetMaster("sesdad.conf");
            puppetMaster.Start();
        }
    }
}
