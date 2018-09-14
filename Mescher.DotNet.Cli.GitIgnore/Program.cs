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

        public static string CacheDirectory =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".dotnet", "tools", ".dotnet-gitignore", "cache");

        const string GitIgnore = ".gitignore";

        const string DefaultUrl = "https://raw.githubusercontent.com/github/gitignore/master/VisualStudio.gitignore";

        [Argument(0, "Output", "The path where the .gitignore file should be saved")]
        public string Output { get; set; }

        [Option(CommandOptionType.SingleOrNoValue, ShortName = "f", LongName = "force",
            Description = "Overwrite any existing files automatically.")]
        public bool Force { get; set; }

        private async Task OnExecuteAsync()
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
                    System.Console.Error.WriteLine("The specified output path does not exist.");
                    Environment.Exit(1);
                }
            }

            string cacheFile = Path.Combine(CacheDirectory, GitIgnore);
            string cacheFileTmp = Path.Combine(CacheDirectory, "_" + GitIgnore);
            string outputFile = Path.Combine(output, GitIgnore);

            Directory.CreateDirectory(Path.GetDirectoryName(cacheFile));

            bool success = false;
            try
            {
                await DownloadFileAsync(DefaultUrl, cacheFileTmp);
                success = true;
            }
            catch (System.Net.WebException)
            {
                if (File.Exists(cacheFile))
                {
                    System.Console.WriteLine($"Warn: Unable to download newer version of {GitIgnore}. Do you have an internet connection?");
                    System.Console.WriteLine("      Using cached version.");
                }
                else
                {
                    System.Console.Error.WriteLine($"Failed to download {GitIgnore}. Do you have an internet connection?");
                    Environment.Exit(1);
                }
            }
            catch (System.Net.Http.HttpRequestException)
            {
                if (File.Exists(cacheFile))
                {
                    System.Console.WriteLine($"Warn: Unable to download newer version of {GitIgnore}. Do you have an internet connection?");
                    System.Console.WriteLine("      Using cached version.");
                }
                else
                {
                    System.Console.Error.WriteLine($"Failed to download {GitIgnore}. Do you have an internet connection?");
                    Environment.Exit(1);
                }
            }
            catch (System.IO.IOException)
            {
                // We failed to download the file to the cache directory, but we could try to download it directly to the output.
                try
                {
                    System.Console.WriteLine($"Warn: Unable to cache file.");

                    await DownloadFileAsync(DefaultUrl, output);

                    // We don't need to proceed with the later copy of the cache file, because we're here.
                    Environment.Exit(1);
                }
                catch
                {
                    System.Console.Error.WriteLine($"Failed to download {GitIgnore}. Could not write file.");
                    Environment.Exit(1);
                }
            }
            catch (Exception)
            {
                System.Console.Error.WriteLine($"Failed to download {GitIgnore}.");
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
                System.Console.Error.WriteLine($"A .gitignore already exists in the output directory. Add --force to overwrite.");
                Environment.Exit(1);
            }

            try
            {
                File.Copy(cacheFile, outputFile, Force);
            }
            catch (IOException)
            {
                System.Console.Error.WriteLine($"Failed to download {GitIgnore}. Do you have permissions to write to \"{output}\"?");
                Environment.Exit(1);
            }
            catch
            {
                System.Console.Error.WriteLine($"Failed to download {GitIgnore}.");
                Environment.Exit(1);
            }
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
