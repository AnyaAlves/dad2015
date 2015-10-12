using System;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using PuppetMasterCommonTypes;

namespace PuppetMasterConsole {
    //<Summary>
    // Puppet Master Text User Interface
    //</Summary>
    public class PuppetMasterConsole {
        //<Summary>
        // Starts connection with Puppet Master Service
        //</Summary>
        private static IExecutioner Connect(String puppetMasterURL) {
            TcpChannel channel = new TcpChannel();
            ChannelServices.RegisterChannel(channel, true);
            return (IExecutioner)Activator.GetObject(
                typeof(IExecutioner),
                puppetMasterURL);
        }
        //<Summary>
        // Send a command to Puppet Master Service for execution
        //</Summary>
        public static void ExecuteCommand(String puppetMasterURL, String command) {
            String reply;
            IExecutioner puppetMasterRemoeObject = Connect(puppetMasterURL);
            reply = puppetMasterRemoeObject.ExecuteCommand(command);
            System.Console.WriteLine(reply);

        }
        //<Summary>
        // Create CLI interface for user interaction with Puppet Master Service
        //</Summary>
        public static void StartCLI(String puppetMasterURL) {
            String command, reply;
            IExecutioner puppetMasterRemoeObject = Connect(puppetMasterURL);
            while (true) {
                command = System.Console.ReadLine();
                reply = puppetMasterRemoeObject.ExecuteCommand(command);
                System.Console.WriteLine(reply);
            }
        }
    }
}
