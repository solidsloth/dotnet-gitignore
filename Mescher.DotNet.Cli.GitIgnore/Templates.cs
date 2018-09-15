using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace Mescher.DotNet.Cli.GitIgnore
{
    [HelpOption]
    [Command("templates")]
    [Subcommand("list", typeof(List))]
    public class Templates
    {
        private int OnExecute(CommandLineApplication app, IConsole console)
        {
            console.WriteLine("The templates command requires an argument");
            console.WriteLine();
            app.ShowHelp();
            return 1;
        }

        [HelpOption]
        [Command(Description = "List available templates")]
        private class List
        {
            [Argument(0, "Search Pattern", "A pattern to filter the results. Only accepts ab*, *bc, and a*c patterns.")]
            public string SearchPattern { get; set; } = "";

            [Option(CommandOptionType.NoValue, LongName = "regex", ShortName = null, Description = "Use regex for searching")]
            public bool IsRegex { get; set; }

            private async Task<int> OnExecuteAsync(CommandLineApplication app, IConsole console)
            {
                var api = new GitHubApiService();

                IEnumerable<string> templates;
                try
                {
                    templates = await api.GetAvailableTemplates(SearchPattern, IsRegex);
                }
                catch (FormatException ex)
                {
                    console.Error.WriteLine($"Failed to search templates: {ex.Message}");
                    console.WriteLine();
                    return 1;
                }
                catch (Exception)
                {
                    console.Error.WriteLine("Failed to get a template listing");
                    console.WriteLine();
                    return 1;
                }

                foreach (string template in templates)
                {
                    console.WriteLine(template);
                }

                return 0;
            }
        }
    }
}
