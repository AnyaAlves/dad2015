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
        private IList<IPuppetMasterURL> puppetMasterRemoteObject;
        private int port;

        public PuppetMaster () {
            puppetMasterRemoteObject = new List<IPuppetMasterURL>();
            port = 1000;
        }

        //<summary>
        // Identify command
        //</summary>
        private String parseLineToCommand(String line) {
            String[] fields = line.Split(' ');
            String command = fields[0];

            switch (fields.Length) {
                case 1:
                    if (command.Equals("Status")) { foreach (IPuppetMasterURL pmro in puppetMasterRemoteObject) pmro.ExecuteStatusCommand(); }
                    break;
                case 2:
                    switch (command[0]) {
                        case 'R':
                            if (command.Equals("RoutingPolicy")) {
                                if (fields[1] == "flooding") {
                                    foreach (IPuppetMasterURL pmro in puppetMasterRemoteObject) pmro.ExecuteFloodingRoutingPolicyCommand();
                                }
                                else if (fields[1] == "filter") {
                                    foreach (IPuppetMasterURL pmro in puppetMasterRemoteObject) pmro.ExecuteFilterRoutingPolicyCommand();
                                }
                            }
                            break;
                        case 'O':
                            if (command.Equals("Ordering")) {
                                if (fields[1] == "NO") {
                                    foreach (IPuppetMasterURL pmro in puppetMasterRemoteObject) pmro.ExecuteNoOrderingCommand();
                                }
                                if (fields[1] == "FIFO") {
                                    foreach (IPuppetMasterURL pmro in puppetMasterRemoteObject) pmro.ExecuteFIFOOrderingCommand();
                                }
                                if (fields[1] == "TOTAL") {
                                    foreach (IPuppetMasterURL pmro in puppetMasterRemoteObject) pmro.ExecuteTotalOrderingCommand();
                                }
                            }
                            break;
                        case 'C':
                            if (command.Equals("Crash")) {
                                foreach (IPuppetMasterURL pmro in puppetMasterRemoteObject) pmro.ExecuteCrashCommand(fields[1]);
                            }
                            break;
                        case 'F':
                            if (command.Equals("Freeze")) {
                                foreach (IPuppetMasterURL pmro in puppetMasterRemoteObject) pmro.ExecuteFreezeCommand(fields[1]);
                            }
                            break;
                        case 'U':
                            if (command.Equals("Unfreeze")) {
                                foreach (IPuppetMasterURL pmro in puppetMasterRemoteObject) pmro.ExecuteUnfreezeCommand(fields[1]);
                            }
                            break;
                        case 'W':
                            if (command.Equals("Wait")) {
                                int integerTime;
                                if (Int32.TryParse(fields[3], out integerTime)) {
                                    foreach (IPuppetMasterURL pmro in puppetMasterRemoteObject) pmro.ExecuteWaitCommand(integerTime);
                                }
                            }
                            break;
                        case 'L':
                            if (command.Equals("LoggingLevel")) {
                                if (fields[1] == "full") {
                                    foreach (IPuppetMasterURL pmro in puppetMasterRemoteObject) pmro.ExecuteFullLoggingLevelCommand();
                                }
                                if (fields[1] == "light") {
                                    foreach (IPuppetMasterURL pmro in puppetMasterRemoteObject) pmro.ExecuteLightLoggingLevelCommand();
                                }
                            }
                            break;
                    }
                    break;
                case 4:
                    if (command.Equals("Site") &&
                        fields[2].Equals("Parent")) {
                            String puppetMasterURL = fields[1] + ":" + (port++).ToString() + "/PuppetMasterURL";
                            CreatePuppetMasterURL(puppetMasterURL);
                            GetRemoteObject(puppetMasterURL);
                            foreach (IPuppetMasterURL pmro in puppetMasterRemoteObject) pmro.ExecuteSiteCommand(
                                fields[1],
                                fields[3]);
                    }
                    else if (command.Equals("Subscriber")) {
                        if (fields[2].Equals("Subscribe")) {
                            foreach (IPuppetMasterURL pmro in puppetMasterRemoteObject) pmro.ExecuteSubscribeCommand(
                                fields[1],
                                fields[3]);
                        }
                        else if (fields[2].Equals("Unsubscribe")) {
                            foreach (IPuppetMasterURL pmro in puppetMasterRemoteObject) pmro.ExecuteUnsubscribeCommand(
                                fields[1],
                                fields[3]);
                        }
                    }
                    break;
                case 8:
                    if (command.Equals("Publisher") &&
                        fields[2].Equals("Publish") &&
                        fields[4].Equals("Ontopic") &&
                        fields[6].Equals("Interval")) {
                        int publishTimes, intervalTimes;
                        if (Int32.TryParse(fields[3], out publishTimes) &&
                           Int32.TryParse(fields[7], out intervalTimes))
                            foreach (IPuppetMasterURL pmro in puppetMasterRemoteObject) pmro.ExecutePublishCommand(
                                fields[1],
                                publishTimes,
                                fields[5],
                                intervalTimes);
                    }
                    else if (command.Equals("Process") &&
                        fields[2].Equals("Is") &&
                        fields[4].Equals("On") &&
                        fields[6].Equals("URL") &&
                        new Regex("^tcp:\\/{2}[\\d\\.]+\\:\\d+\\/\\w*$").IsMatch(fields[7])) {
                        if (fields[3].Equals("broker")) {
                            foreach (IPuppetMasterURL pmro in puppetMasterRemoteObject) pmro.ExecuteBrokerCommand(
                                fields[1],
                                fields[5],
                                fields[7]);
                        }
                        else if (fields[3].Equals("publisher")) {
                            foreach (IPuppetMasterURL pmro in puppetMasterRemoteObject) pmro.ExecutePublisherCommand(
                                fields[1],
                                fields[5],
                                fields[7]);
                        }
                        else if (fields[3].Equals("subscriber")) {
                            foreach (IPuppetMasterURL pmro in puppetMasterRemoteObject) pmro.ExecuteSubscriberCommand(
                                fields[1],
                                fields[5],
                                fields[7]);
                        }
                    }
                    break;
            }

            return "test";
        }
        private void CreatePuppetMasterURL(String puppetMasterURL) {
            String[] args = new [] {puppetMasterURL};
            Action<String[]> thread = new Action<String[]>(SESDAD.PuppetMaster.Program.Main);
            thread.BeginInvoke(args, null, null);
            System.Console.WriteLine("Ok... Now what?");
            System.Console.ReadLine();
        }
        //<summary>
        // Starts connection with Puppet Master Service
        //</summary>
        private void GetRemoteObject(String puppetMasterURL) {
            puppetMasterRemoteObject.Add((IPuppetMasterURL)Activator.GetObject(
                typeof(IPuppetMasterURL),
                puppetMasterURL));
        }
        //<summary>
        // Starts connection with Puppet Master Service
        //</summary>
        public void Connect() {
            TcpChannel channel = new TcpChannel(port++);
            ChannelServices.RegisterChannel(channel, true);
            PuppetMasterURL puppetMasterURL = new PuppetMasterURL();
            RemotingServices.Marshal(
                puppetMasterURL,
                "PuppetMasterURL",
                typeof(PuppetMasterURL));
            puppetMasterRemoteObject.Add((IPuppetMasterURL)puppetMasterURL);
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
