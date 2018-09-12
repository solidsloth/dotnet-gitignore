using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;

namespace Mescher.DotNet.Cli.GitIgnore
{
    class Program
    {
        public static int Main(string[] args)
            => CommandLineApplication.Execute<Program>(args);

        public static string ConfigPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "dispatch-gitignore");

        const string DefaultUrl = "https://raw.githubusercontent.com/github/gitignore/master/VisualStudio.gitignore";

        [Argument(0, "Output", "The path where the .gitignore file should be saved")]
        public string Output { get; set; }

        private async Task OnExecuteAsync()
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile(ConfigPath, true)
                .Build();

            string output = Path.GetFullPath(Output ?? Environment.CurrentDirectory);

            if (!Directory.Exists(output))
            {
                System.Console.Error.WriteLine("The specified output path does not exist.");
                Environment.Exit(1);
            }

            using (var client = new WebClient())
            {
                await client.DownloadFileTaskAsync(
                    new Uri(DefaultUrl),
                    Path.Combine(output, ".gitignore"));
            }
        }
    }
}
