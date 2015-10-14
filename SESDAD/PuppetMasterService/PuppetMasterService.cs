using System;
using System.ServiceProcess;

namespace PuppetMasterService {
    public partial class PuppetMasterService : ServiceBase {
        private PuppetMaster puppetMaster;

        public PuppetMasterService() {
            InitializeComponent();
        }

        protected override void OnStart(string[] args) {
            if (args.Length == 2) {
                puppetMaster = new PuppetMaster();
                puppetMaster.Connect();
            }
        }

        protected override void OnStop() {
        }
    }
}
