using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mescher.DotNet.Cli.GitIgnore
{
    /// <summary>
    /// Represents a response from the a tree endpoint of the GitHubApi.
    /// </summary>
    public class GitTreeResponse
    {
        /// <summary>
        /// The SHA hash of the commit.
        /// </summary>
        public string Sha { get; set; }

        /// <summary>
        /// A self referential link back to the resource.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// A the files that exist in this tree..
        /// </summary>
        [JsonProperty("tree")]
        public IEnumerable<GitFile> Files { get; set; }
    }
}
