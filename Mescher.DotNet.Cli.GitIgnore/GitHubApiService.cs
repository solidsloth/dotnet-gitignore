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
    public class GitHubApiService
    {
        const string TemplateUrl = "https://api.github.com/repos/github/gitignore/git/trees/master";

        public async Task<IEnumerable<string>> GetAvailableTemplates(
            string searchPattern = "",
            bool isRegex = false,
            CancellationToken ct = default(CancellationToken))
        {
            IEnumerable<GitFile> files = (await TemplateUrl
                .WithHeaders(new { User_Agent = "dotnet-gitignore" })
                .GetJsonAsync<GitTreeResponse>(ct)).Files
                .Where(t => t.Path.EndsWith(".gitignore"));

            if (isRegex)
            {
                Regex regex = new Regex(searchPattern);
                files = files.Where(t => regex.IsMatch(t.Path));
            }
            else
            {
                files = files.WhereGitFileMatches(searchPattern);
            }

            return files.Select(f => "    " + f.Path.Replace(".gitignore", string.Empty));
        }
    }
}
