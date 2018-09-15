using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Flurl;
using Flurl.Http;

namespace Mescher.DotNet.Cli.GitIgnore
{
    [Subcommand("templates", typeof(Templates))]
    class Program
    {
        public static int Main(string[] args)
            => CommandLineApplication.Execute<Program>(args);

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

        private async Task<int> OnExecuteAsync(CommandLineApplication app, IConsole console)
        {
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

            Directory.CreateDirectory(CacheDirectory);

            if (string.IsNullOrWhiteSpace(Template))
            {
                Template = DefaultTemplate;
            }
            else
            {
                if (!Template.EndsWith(".gitignore"))
                {
                    Template = Template + ".gitignore";
                }
            }

            string cacheFile = Path.Combine(CacheDirectory, Template);
            string cacheFileTmp = Path.Combine(CacheDirectory, "_" + Template);
            string outputFile = Path.Combine(output, GitIgnore);

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

            if (File.Exists(outputFile) && !Force)
            {
                console.Error.WriteLine($"A .gitignore already exists in the output directory. Add --force to overwrite.");
                return 1;
            }

            try
            {
                File.Copy(cacheFile, outputFile, Force);
            }
            catch (IOException ex)
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
