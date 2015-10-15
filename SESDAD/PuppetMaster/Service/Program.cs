using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SESDAD.PuppetMaster.Library;

namespace SESDAD.PuppetMaster.Service {
    public static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static void Main(string[] args) {
            //ServiceBase[] ServicesToRun;
            //ServicesToRun = new ServiceBase[] 
            //{ 
            //    new PuppetMasterService() 
            //};
            //ServiceBase.Run(ServicesToRun);
            PuppetMasterService puppetMaster = new PuppetMasterService();
            puppetMaster.Connect();
        }
    }
}
