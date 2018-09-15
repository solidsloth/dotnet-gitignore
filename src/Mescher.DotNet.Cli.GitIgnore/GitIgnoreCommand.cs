using System;
using System.Linq;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Flurl;
using Flurl.Http;
using System.Reflection;

namespace Mescher.DotNet.Cli.GitIgnore
{
    [Subcommand("templates", typeof(TemplatesCommand))]
    class GitIgnoreCommand
    {
        public static int Main(string[] args)
            => CommandLineApplication.Execute<GitIgnoreCommand>(args);

        public static string CacheDirectory =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dotnet", "tools", ".dotnet-gitignore", "cache");

        const string GitIgnore = ".gitignore";

        const string BaseUrl = "https://raw.githubusercontent.com/github/gitignore/master/";

        const string DefaultTemplate = "VisualStudio.gitignore";

        [Argument(0, "Output", "The path where the .gitignore file should be saved")]
        public string Output { get; set; }

        [Option(CommandOptionType.NoValue, ShortName = "f", LongName = "force",
            Description = "Overwrite any existing files automatically.")]
        public bool Force { get; set; }

        [Option(CommandOptionType.SingleValue, ShortName = "t", LongName = "template",
            Description = "Specifies the .gitignore template to download. (Default is VisualStudio)")]
        public string Template { get; set; }

        [Option(CommandOptionType.NoValue, ShortName = "a", LongName = "append",
            Description = "Append the template to the .gitignore file if it exists")]
        public bool Append { get; set; }

        [Option(CommandOptionType.NoValue, ShortName = "v", LongName = "version",
            Description = "Displays version information")]
        public bool Version { get; set; }

        private async Task<int> OnExecuteAsync(CommandLineApplication app, IConsole console)
        {
            // If the version flag was given, print the version and exit.
            if (Version)
            {
                PrintVersion(console);
                return 0;
            }

            string output = Path.GetFullPath(Output ?? Environment.CurrentDirectory);

            if (!Directory.Exists(output))
            {
                if (Force)
                {
                    Directory.CreateDirectory(output);
                }
                else
                {
                    console.Error.WriteLine("The specified output path does not exist.");
                    return 1;
                }
            }

            string outputFile = Path.Combine(output, GitIgnore);

            if (File.Exists(outputFile) && !Force && !Append)
            {
                console.Error.WriteLine($"A .gitignore already exists in the output directory. Add --force to overwrite or --append to append the existing .gitignore.");
                return 1;
            }

            Directory.CreateDirectory(CacheDirectory);

            if (string.IsNullOrWhiteSpace(Template))
            {
                Template = DefaultTemplate;
            }
            else
            {
                if (Template.EndsWith(".gitignore"))
                {
                    Template = Template.Replace(".gitignore", string.Empty);
                }

                var templateService = new TemplateService();
                string templateName = (await templateService.GetAvailableTemplates())
                    .FirstOrDefault(t => string.Equals(t, Template, StringComparison.OrdinalIgnoreCase));

                if (templateName == null)
                {
                    console.Error.WriteLine($"The template '{Template}' was not found.");
                    return 1;
                }

                Template = templateName + ".gitignore";
            }

            string cacheFile = Path.Combine(CacheDirectory, Template);
            string cacheFileTmp = Path.Combine(CacheDirectory, "_" + Template);

            bool success = false;
            try
            {
                await BaseUrl.AppendPathSegment(Template).DownloadFileAsync(CacheDirectory, "_" + Template);
                success = true;
            }
            catch (System.Net.WebException)
            {
                if (File.Exists(cacheFile))
                {
                    console.WriteLine($"Warn: Unable to download newer version of {GitIgnore}. Do you have an internet connection?");
                    console.WriteLine("      Using cached version.");
                }
                else
                {
                    console.Error.WriteLine($"Failed to download {GitIgnore}. Do you have an internet connection?");
                    return 1;
                }
            }
            catch (System.Net.Http.HttpRequestException)
            {

            }
            catch (System.IO.IOException)
            {
                // We failed to download the file to the cache directory, but we could try to download it directly to the output.
                try
                {
                    console.WriteLine($"Warn: Unable to cache file.");

                    await BaseUrl.AppendPathSegment(Template).DownloadFileAsync(CacheDirectory, GitIgnore);

                    // We don't need to proceed with the later copy of the cache file, because we're here.
                    return 1;
                }
                catch
                {
                    console.Error.WriteLine($"Failed to download {GitIgnore}. Could not write file.");
                    return 1;
                }
            }
            catch (FlurlHttpException ex)
            {
                if (!ex.Call.Completed)
                {
                    if (File.Exists(cacheFile))
                    {
                        console.WriteLine($"Warn: Unable to download newer version of {GitIgnore}. Do you have an internet connection?");
                        console.WriteLine("      Using cached version.");
                    }
                    else
                    {
                        console.Error.WriteLine($"Failed to download {GitIgnore}. Do you have an internet connection?");
                        return 1;
                    }
                }
                else
                {
                    switch (ex?.Call?.Response?.StatusCode ?? HttpStatusCode.InternalServerError)
                    {
                        case HttpStatusCode.NotFound:
                            console.Error.WriteLine($"The template '{Template}' was not found.");
                            return 1;
                        default:
                            console.Error.WriteLine($"Failed to download {GitIgnore}.");
                            return 1;
                    }
                }
            }
            catch (Exception)
            {
                console.Error.WriteLine($"Failed to download {GitIgnore}.");
                Environment.Exit(1);
            }

            if (success)
            {
                try
                {
                    if (File.Exists(cacheFile))
                    {
                        File.Delete(cacheFile);
                    }
                    File.Move(cacheFileTmp, cacheFile);
                }
                catch (Exception)
                {

                }
            }

            try
            {
                if (Append)
                {
                    File.AppendAllLines(outputFile, File.ReadAllLines(cacheFile));
                }
                else
                {
                    File.Copy(cacheFile, outputFile, Force);
                }
            }
            catch (IOException)
            {
                console.Error.WriteLine($"Failed to download {GitIgnore}. Do you have permissions to write to \"{output}\"?");
                return 1;
            }
            catch
            {
                console.Error.WriteLine($"Failed to download {GitIgnore}.");
                return 1;
            }

            return 0;
        }

        /// <summary>
        /// Prints the current assembly version.
        /// </summary>
        /// <param name="console">The console to write to.</param>
        public void PrintVersion(IConsole console)
        {
            // Get the current version.
            Version version = Assembly.GetExecutingAssembly().GetName().Version;

            // Write the first 3 fields of the version.
            console.WriteLine(version.ToString(3));
        }

        ///<summary>
        /// Downloads the file from the given
        ///</summary>
        public async Task DownloadFileAsync(string url, string fileName)
        {
            using (var client = new WebClient())
            {
                // Save the file to the config ca
                try
                {
                    await client.DownloadFileTaskAsync(new Uri(url), fileName);
                }
                catch (System.Net.WebException ex)
                {
                    throw ex.InnerException ?? ex;
                }
            }
        }
    }
}
