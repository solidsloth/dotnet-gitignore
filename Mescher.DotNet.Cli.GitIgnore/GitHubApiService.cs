using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Threading;
using System.Text.RegularExpressions;

namespace Mescher.DotNet.Cli.GitIgnore
{
    /// <summary>
    /// A Service which provides operations for accessing the GitHubApi.
    /// </summary>
    public class GitHubApiService
    {
        /// <summary>
        /// The GitHub api endpoint where the most recent file list can be found.
        /// </summary>
        const string TemplateUrl = "https://api.github.com/repos/github/gitignore/git/trees/master";

        /// <summary>
        /// Searches GitHub for a listing of available .gitignore templates.
        /// </summary>
        /// <param name="searchPattern">The pattern used for searching.</param>
        /// <param name="isRegex">Whether or not the search uses Regular Expressions.</param>
        /// <param name="ct">A token for cancelling the async operation.</param>
        public async Task<IEnumerable<string>> GetAvailableTemplates(
            string searchPattern = "",
            bool isRegex = false,
            CancellationToken ct = default(CancellationToken))
        {
            // Perform a GET request to the GitHub API to get a file listing of *.gitignore files.
            IEnumerable<GitFile> files = (await TemplateUrl
                .WithHeaders(new { User_Agent = "dotnet-gitignore" }) // A 403 is returned if this is not defined.
                .GetJsonAsync<GitTreeResponse>(ct)).Files
                .Where(t => t.Path.EndsWith(".gitignore"));

            // Check if we have a search string.
            if (!string.IsNullOrWhiteSpace(searchPattern))
            {
                // Check if this is a regex search.
                if (isRegex)
                {
                    // Compile a regex and compare all results to it.
                    Regex regex = new Regex(searchPattern);
                    files = files.Where(t => regex.IsMatch(t.Path));
                }
                else
                {
                    // Filter using the search pattern.
                    files = files.WhereGitFileMatches(searchPattern);
                }
            }

            // Return just the file names.
            return files.Select(f => f.Path.Replace(".gitignore", string.Empty));
        }
    }
}
