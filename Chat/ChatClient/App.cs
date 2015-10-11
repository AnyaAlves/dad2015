using System;
using System.Windows.Forms;

namespace ChatClient {
    static class App {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
