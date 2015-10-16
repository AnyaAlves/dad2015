using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using System.Net.Sockets;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Text.RegularExpressions;
using SESDAD.PuppetMaster.CommonTypes;
using System.Threading;

using System.Diagnostics;

namespace SESDAD.PuppetMaster {
    public class PuppetMaster {
        private IList<IPuppetMasterService> puppetMasters;
        private int port;

        public PuppetMaster() {
            puppetMasters = new List<IPuppetMasterService>();
            port = 1000;
        }

        //<summary>
        // Starts connection with Puppet Master Service
        //</summary>
        public void Connect() {
            TcpChannel channel = new TcpChannel(port++);
            ChannelServices.RegisterChannel(channel, true);
            PuppetMasterService puppetMasterService = new PuppetMasterService();
            RemotingServices.Marshal(
                puppetMasterService,
                "puppetMasterService",
                typeof(PuppetMasterService));
            puppetMasters.Add((IPuppetMasterService)puppetMasterService);
        }

        public void ExecuteConfigurationFile() {
            String line;
            StreamReader file = new StreamReader("sesdadrc");
            while ((line = file.ReadLine()) != null) {
                parseLineToCommand(line);
            }
            file.Close();
        }

        //<summary>
        // Create CLI interface for user interaction with Puppet Master Service
        //</summary>
        public void StartCLI() {
            String command, reply;
            while (true) {
                command = System.Console.ReadLine();
                reply = parseLineToCommand(command);
                System.Console.WriteLine(reply);
            }
        }

        //<summary>
        // Identify command
        //</summary>
        private String parseLineToCommand(String line) {
            String[] fields = line.Split(' ');
            String command = fields[0];

            if (fields.Length==1 && command.Equals("Status")) {
                foreach (IPuppetMasterService puppetMasterService in puppetMasters)
                    puppetMasterService.ExecuteStatusCommand();
            }
            if (fields.Length==2 && command.Equals("RoutingPolicy")) {
                if (fields[1] == "flooding") {
                    foreach (IPuppetMasterService puppetMasterService in puppetMasters)
                        puppetMasterService.ExecuteFloodingRoutingPolicyCommand();
                } else if (fields[1] == "filter") {
                    foreach (IPuppetMasterService puppetMasterService in puppetMasters)
                        puppetMasterService.ExecuteFilterRoutingPolicyCommand();
                }
            }
            if (fields.Length==2 && command.Equals("Ordering")) {
                if (fields[1] == "NO") {
                    foreach (IPuppetMasterService puppetMasterService in puppetMasters)
                        puppetMasterService.ExecuteNoOrderingCommand();
                }else if (fields[1] == "FIFO") {
                    foreach (IPuppetMasterService puppetMasterService in puppetMasters)
                        puppetMasterService.ExecuteFIFOOrderingCommand();
                }else if (fields[1] == "TOTAL") {
                    foreach (IPuppetMasterService puppetMasterService in puppetMasters)
                        puppetMasterService.ExecuteTotalOrderingCommand();
                }
            }
            if (fields.Length==2 && command.Equals("Crash")) {
                foreach (IPuppetMasterService puppetMasterService in puppetMasters)
                    puppetMasterService.ExecuteCrashCommand(fields[1]);
            }
            if (fields.Length==2 && command.Equals("Freeze")) {
                foreach (IPuppetMasterService puppetMasterService in puppetMasters)
                    puppetMasterService.ExecuteFreezeCommand(fields[1]);
            }
            if (fields.Length==2 && command.Equals("Unfreeze")) {
                foreach (IPuppetMasterService puppetMasterService in puppetMasters)
                    puppetMasterService.ExecuteUnfreezeCommand(fields[1]);
            }
            if (fields.Length == 2 && command.Equals("Wait")) {
                int integerTime;
                if (Int32.TryParse(fields[1], out integerTime)) {
                    foreach (IPuppetMasterService puppetMasterService in puppetMasters)
                        puppetMasterService.ExecuteWaitCommand(integerTime);
                }
                if (fields.Length == 2 && command.Equals("LoggingLevel")) {
                    if (fields[1] == "full") {
                        foreach (IPuppetMasterService puppetMasterService in puppetMasters)
                            puppetMasterService.ExecuteFullLoggingLevelCommand();
                    }
                    else if (fields[1] == "light") {
                        foreach (IPuppetMasterService puppetMasterService in puppetMasters)
                            puppetMasterService.ExecuteLightLoggingLevelCommand();
                    }
                }
            }
            if (fields.Length == 4 && command.Equals("Site") && fields[2].Equals("Parent")) {
                String puppetMasterURL = "tcp://" + fields[1] + ":" + (port++).ToString() + "/puppetMasterService";
                CreatePuppetMasterService(puppetMasterURL);
                foreach (IPuppetMasterService puppetMasterService in puppetMasters)
                    puppetMasterService.ExecuteSiteCommand(fields[1], fields[3]);
            }
            if (fields.Length == 4 && command.Equals("Subscriber")) {
                if (fields[2].Equals("Subscribe")) {
                    foreach (IPuppetMasterService puppetMasterService in puppetMasters)
                        puppetMasterService.ExecuteSubscribeCommand(fields[1], fields[3]);
                }
                else if (fields[2].Equals("Unsubscribe")) {
                    foreach (IPuppetMasterService puppetMasterService in puppetMasters)
                        puppetMasterService.ExecuteUnsubscribeCommand(fields[1], fields[3]);
                }
            }
            if (fields.Length == 8 && command.Equals("Publisher") &&
                        fields[2].Equals("Publish") &&
                        fields[4].Equals("Ontopic") &&
                        fields[6].Equals("Interval")) {
                int publishTimes, intervalTimes;
                if (Int32.TryParse(fields[3], out publishTimes) && Int32.TryParse(fields[7], out intervalTimes)) {
                    foreach (IPuppetMasterService puppetMasterService in puppetMasters)
                        puppetMasterService.ExecutePublishCommand(
                                fields[1],
                                publishTimes,
                                fields[5],
                                intervalTimes);
                }
            }
            if (fields.Length == 8 && command.Equals("Process") &&
                        fields[2].Equals("Is") &&
                        fields[4].Equals("On") &&
                        fields[6].Equals("URL") &&
                        new Regex("^tcp://\\w+:\\d\\d\\d\\d/\\w+$").IsMatch(fields[7])) {
                if (fields[3].Equals("broker")) {
                    foreach (IPuppetMasterService puppetMasterService in puppetMasters)
                        puppetMasterService.ExecuteBrokerCommand(
                                fields[1],
                                fields[5],
                                fields[7]);
                } else if (fields[3].Equals("publisher")) {
                    foreach (IPuppetMasterService puppetMasterService in puppetMasters)
                        puppetMasterService.ExecutePublisherCommand(
                                fields[1],
                                fields[5],
                                fields[7]);
                } else if (fields[3].Equals("subscriber")) {
                    foreach (IPuppetMasterService puppetMasterService in puppetMasters)
                        puppetMasterService.ExecuteSubscriberCommand(
                                fields[1],
                                fields[5],
                                fields[7]);
                }
            }
            return "test";
        }

        //wtf is this
        private void CreatePuppetMasterService(String puppetMasterURL) {
        /*    String[] args = new[] { puppetMasterURL }; //what
            Action<String[]> thread = new Action<String[]>(SESDAD.PuppetMaster.Program.Main); //?????
            thread.BeginInvoke(args, null, null);
            puppetMasters.Add((IPuppetMasterService)Activator.GetObject(
                typeof(IPuppetMasterService),
                puppetMasterURL));
           
            System.Console.WriteLine("Ok... Now what?");
            System.Console.ReadLine();*/
        }
    }

    public static class Program {
        //<summary>
        // Entry Point
        //</summary>
        public static void Main(string[] args) {
            PuppetMaster puppetMaster = new PuppetMaster();
            puppetMaster.Connect();
            puppetMaster.ExecuteConfigurationFile();
            puppetMaster.StartCLI();
        }
    }
}
