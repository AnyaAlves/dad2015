namespace PuppetMasterService {
    partial class ProjectInstaller {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.PuppetMasterServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.PuppetMasterServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // PuppetMasterServiceProcessInstaller
            // 
            this.PuppetMasterServiceProcessInstaller.Password = null;
            this.PuppetMasterServiceProcessInstaller.Username = null;
            // 
            // PuppetMasterServiceInstaller
            // 
            this.PuppetMasterServiceInstaller.ServiceName = "PuppetMasterService";
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.PuppetMasterServiceProcessInstaller,
            this.PuppetMasterServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller PuppetMasterServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller PuppetMasterServiceInstaller;
    }
}