using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace Mescher.DotNet.Cli.GitIgnore
{
    /// <summary>
    /// Represents the templates command.
    /// <para/>
    /// Called when using "gitignore templates"
    /// </summary>
    [HelpOption]
    [Command("templates")]
    [Subcommand("list", typeof(ListCommand))]
    public class TemplatesCommand
    {
        /// <summary>
        /// The action executed when running the templates command with no subcommands.
        /// </summary>
        /// <param name="app">The current console app.</param>
        /// <param name="console">The output console.</param>
        /// <returns></returns>
        private int OnExecute(CommandLineApplication app, IConsole console)
        {
            console.WriteLine("The templates command requires an argument");
            console.WriteLine();
            app.ShowHelp();
            return 1;
        }

        /// <summary>
        /// Represends the list subcommand.
        /// </summary>
        [HelpOption]
        [Command(Description = "List available templates")]
        private class ListCommand
        {
            /// <summary>
            /// The indent to be prepended to the listing.
            /// </summary>
            const string Indent = "    ";

            /// <summary>
            /// The search pattern used to list packages. Can be a regex string, or a wildcard string.
            /// </summary>
            /// <remarks>
            /// Currently supported wildcard strings are a*, *c, a*c, and *b*.
            /// </remarks>
            /// <value></value>
            [Argument(0, "Search Pattern", "A pattern to filter the results. Only accepts ab*, *bc, and a*c patterns.")]
            public string SearchPattern { get; set; } = "";

            /// <summary>
            /// A flag which denotes whether or not the search pattern is a regex pattern.
            /// </summary>
            /// <value></value>
            [Option(CommandOptionType.NoValue, LongName = "regex", ShortName = null, Description = "Use regex for searching")]
            public bool IsRegex { get; set; }

            /// <summary>
            /// The action executed when running the lists subcommand.
            /// </summary>
            /// <param name="app">The current console app.</param>
            /// <param name="console">The output console.</param>
            /// <returns></returns>
            private async Task<int> OnExecuteAsync(CommandLineApplication app, IConsole console)
            {
                // Create a new template service.
                var templateService = new TemplateService();

                IEnumerable<string> templates;
                try
                {
                    // Get an available template listing.
                    templates = await templateService.GetAvailableTemplates(SearchPattern, IsRegex);
                }
                catch (FormatException ex)
                {
                    // The search pattern format was incorrect.
                    console.Error.WriteLine($"Failed to search templates: {ex.Message}");
                    console.WriteLine();
                    return 1;
                }
                catch (Exception)
                {
                    // This is an unhandled exception.
                    console.Error.WriteLine("Failed to get a template listing");
                    console.WriteLine();
                    return 1;
                }

                // Write each of the templates to the console.
                foreach (string template in templates)
                {
                    console.WriteLine(Indent + template);
                }

                // Return a success status code.
                return 0;
            }
        }
    }
}
