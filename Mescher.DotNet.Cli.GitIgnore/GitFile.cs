using Newtonsoft.Json;

namespace Mescher.DotNet.Cli.GitIgnore
{
    public class GitFile
    {
        public string Path { get; set; }

        public string Mode { get; set; }

        public string Type { get; set; }

        public string Sha { get; set; }

        public string Url { get; set; }

        public int? Size { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
