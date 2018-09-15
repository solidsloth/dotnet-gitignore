using Newtonsoft.Json;

namespace Mescher.DotNet.Cli.GitIgnore
{
    /// <summary>
    /// Represents a file in a Git Tree.
    /// </summary>
    public class GitFile
    {
        /// <summary>
        /// The path to the file.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// The file permissions.
        /// </summary>
        public string Mode { get; set; }

        /// <summary>
        /// The file type.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The file hash.
        /// </summary>
        public string Sha { get; set; }

        /// <summary>
        /// A link to the API resource for this file.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The file size. Null if a directory.
        /// </summary>
        public int? Size { get; set; }

        /// <summary>
        /// Overrides to string to display a JSON string of the object.
        /// </summary>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
