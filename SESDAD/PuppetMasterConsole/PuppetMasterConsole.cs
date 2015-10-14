using System;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using PuppetMasterCommonTypes;

namespace PuppetMasterConsole {
    //<Summary>
    // Puppet Master Text User Interface
    //</Summary>
    public class PuppetMasterConsole {
        private IExecutioner puppetMasterRemoteObject;

        //<Summary>
        // Starts connection with Puppet Master Service
        //</Summary>
        public void Connect(String puppetMasterURL) {
            TcpChannel channel = new TcpChannel();
            ChannelServices.RegisterChannel(channel, true);
            puppetMasterRemoteObject = (IExecutioner)Activator.GetObject(
                typeof(IExecutioner),
                puppetMasterURL);
        }
        //<Summary>
        // Send a command to Puppet Master Service for execution
        //</Summary>
        public void ExecuteCommand(String command) {
            String reply;
            reply = puppetMasterRemoteObject.ExecuteCommand(command);
            System.Console.WriteLine(reply);

        }
        //<Summary>
        // Create CLI interface for user interaction with Puppet Master Service
        //</Summary>
        public void StartCLI() {
            String command, reply;
            while (true) {
                command = System.Console.ReadLine();
                reply = puppetMasterRemoteObject.ExecuteCommand(command);
                System.Console.WriteLine(reply);
            }
        }
    }
}
