using System;
using System.Collections.Generic;
using System.Linq;

namespace Mescher.DotNet.Cli.GitIgnore
{
    /// <summary>
    /// Contains extension for <see cref="IEnumerable"/> and <see cref="IEnumerable{T}"/>.
    /// </summary>
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Filters the <see cref="IEnumerable"/> of <see cref="GitFile"/> according to
        /// the given search pattern.
        /// </summary>
        /// <param name="files">The file list to filter.</param>
        /// <param name="searchPattern">The search pattern to filter with.</param>
        /// <returns>A filtered <see cref="IEnumerable{GitFile}"/>.</returns>
        public static IEnumerable<GitFile> WhereGitFileMatches(this IEnumerable<GitFile> files, string searchPattern)
        {
            // The generic format exception.
            var fex = new FormatException("List filtering works with simple patterns such as: ab*, a*b, *bc, and *b*");

            // If the search pattern starts and ends with *, then we should use Contains.
            if (searchPattern[0] == '*' && searchPattern[searchPattern.Length - 1] == '*')
            {
                // Remove leading and trailing *.
                searchPattern = searchPattern.TrimStart('*').TrimEnd('*');

                // If there is still a * in the string after trimming *, then we can conclude that
                // there is a * in the middle of the string as well, and we do not currently support filtering this.
                if (searchPattern.Contains("*")) throw fex;

                // Filter where the file name without the .gitignore extension contains the search string.
                files = files.Where(f => f.Path.Replace(".gitignore", string.Empty).Contains(searchPattern));
            }
            else
            {
                // Get the index of the first *.
                int wildCardIndex = searchPattern.IndexOf('*');

                // If the last index does not match the first, then there are multiple *'s, which did not
                // occur as the first and last characters, and we do not currently support filtering this.
                if (wildCardIndex != searchPattern.LastIndexOf('*')) throw fex;

                // By default the ends with starts at the character after the wildcard.
                int endStart = wildCardIndex + 1;

                // Check if there are no wildcards.
                if (wildCardIndex < 0)
                {
                    // The index will be set to the end of the string.
                    wildCardIndex = searchPattern.Length;

                    // The ends with search will start at the beginning.
                    endStart = 0;
                }

                // Get the expected StartsWith and EndsWith strings.
                string starts = searchPattern.Substring(0, Math.Max(0, wildCardIndex));
                string ends = searchPattern.Substring(Math.Min(endStart, searchPattern.Length));

                // Do a case-insensitive search based on the StartsWith and EndsWith strings.
                files = files.Where(t => t.Path.StartsWith(starts, StringComparison.OrdinalIgnoreCase)
                    && t.Path.Replace(".gitignore", string.Empty).EndsWith(ends, StringComparison.OrdinalIgnoreCase));
            }

            // Return the filtered files.
            return files;
        }
    }
}
