using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using CommandLine;
using CommandLine.Text;

namespace PuppetMasterConsole {
    public class Program {
        //<Summary>
        // Entry Point
        //</Summary>
        public static void Main(String[] args) {
            //ParserSettings parserSettings = new ParserSettings();
            //parserSettings.CaseSensitive = true;
            //parserSettings.IgnoreUnknownArguments = false;
            //parserSettings.HelpWriter = null;

            //Parser parser = new Parser(parserSettings);

            if (args.Length ==2){ //parser.ParseArguments(args, new Options())) {
                PuppetMasterConsole pupperMasterConsole = new PuppetMasterConsole();

                pupperMasterConsole.Connect(args[1]);
                // Se for só um comando
                //pupperMasterConsole.ExecuteCommand("Test");
                // Se for CLI
                pupperMasterConsole.StartCLI();
            }
        }
    }
    //class Options {
    //    [Option('c', "cli", DefaultValue = true, HelpText = "Calls command line interface")]
    //    public bool callCLI { get; set; }

    //    [Option("h", "host", HelpText = "The maximum number of bytes to process.")]
    //    public int getPuppetMasterURL { get; set; }

    //    [Option('c', "command", null, HelpText = "Print details during execution.")]
    //    public bool getCommand { get; set; }

    //    [HelpOption]
    //    public string GetUsage() {
    //        // this without using CommandLine.Text
    //        //  or using HelpText.AutoBuild
    //        var usage = new StringBuilder();
    //        usage.AppendLine("Quickstart Application 1.0");
    //        usage.AppendLine("Read user manual for usage instructions...");
    //        return usage.ToString();
    //    }
    //}
}
