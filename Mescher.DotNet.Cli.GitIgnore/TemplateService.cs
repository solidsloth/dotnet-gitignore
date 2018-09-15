using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mescher.DotNet.Cli.GitIgnore
{
    public class TemplateService
    {

        /// <summary>
        /// Searches for a listing of available .gitignore templates.
        /// </summary>
        /// <param name="searchPattern">The pattern used for searching.</param>
        /// <param name="isRegex">Whether or not the search uses Regular Expressions.</param>
        /// <param name="ct">A token for cancelling the async operation.</param>
        public async Task<IEnumerable<string>> GetAvailableTemplates(
            string searchPattern = "",
            bool isRegex = false,
            CancellationToken ct = default(CancellationToken))
        {
            // For now we just search the github API for templates, but at some point we could
            // search for custom templates that have been added.
            var api = new GitHubApiService();
            return await api.GetAvailableTemplates(searchPattern, isRegex, ct);
        }
    }
}
