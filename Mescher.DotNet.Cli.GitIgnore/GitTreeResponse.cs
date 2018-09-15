using System.Collections.Generic;
using Newtonsoft.Json;

namespace Mescher.DotNet.Cli.GitIgnore
{
    public class GitTreeResponse
    {
        public string Sha { get; set; }

        public string Url { get; set; }

        [JsonProperty("tree")]
        public IEnumerable<GitFile> Files { get; set; }
    }
}
