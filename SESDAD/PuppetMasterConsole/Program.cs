using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMasterConsole {
    public class Program {
        //<Summary>
        // Entry Point
        //</Summary>
        public static void Main(String[] args) {
            // Se for só um comando
            PuppetMasterConsole.ExecuteCommand("test", "Test");
            // Se for CLI
            PuppetMasterConsole.StartCLI("test");
        }
    }
}
