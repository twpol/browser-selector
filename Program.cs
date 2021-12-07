using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace Browser_Selector
{
    class Program
    {
        static void Main(string[] args)
        {
            var (command, arguments) = SplitCommandArgument(Environment.CommandLine);

            var executableFilePath = Process.GetCurrentProcess().MainModule.FileName;
            var configFilePath = Path.Combine(Path.GetDirectoryName(executableFilePath), "config.json");
            var config = new ConfigurationBuilder()
                .AddJsonFile(configFilePath)
                .Build();

            foreach (var browser in config.GetChildren().OrderBy(browser => -(browser["Url"] ?? "").Length))
            {
                var hostname = new Regex(browser["Hostname"] ?? ".");
                if (!hostname.IsMatch(Environment.MachineName))
                {
                    continue;
                }

                var url = new Regex(browser["Url"] ?? ".");
                var isMatch = args.Any(arg => url.IsMatch(arg));
                if (!isMatch)
                {
                    continue;
                }

                Console.WriteLine($"Launching with {browser.Key}...");

                var (launchCommand, launchArguments) = SplitCommandArgument($"{browser["Launch"]} {arguments}");
                Process.Start(launchCommand, launchArguments);
                break;
            }
        }

        static (string, string) SplitCommandArgument(string commandLine)
        {
            if (commandLine[0] == '"')
            {
                var argumentsStart = commandLine.IndexOf('"', 1) + 1;
                return (commandLine.Substring(1, argumentsStart - 2), commandLine.Substring(argumentsStart).Trim());
            }
            else
            {
                var argumentsStart = commandLine.IndexOf(' ');
                return (commandLine.Substring(0, argumentsStart), commandLine.Substring(argumentsStart).Trim());
            }
        }
    }
}
