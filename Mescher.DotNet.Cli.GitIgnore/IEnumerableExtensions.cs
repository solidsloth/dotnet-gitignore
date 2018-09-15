using System;
using System.Collections.Generic;
using System.Linq;

namespace Mescher.DotNet.Cli.GitIgnore
{
    public static class ListFilterer
    {
        public static IEnumerable<GitFile> WhereGitFileMatches(this IEnumerable<GitFile> files, string searchPattern)
        {
            FormatException fex = new FormatException("List filtering works with simple patterns such as: ab*, a*b, *bc, and *b*");

            if (searchPattern[0] == '*' && searchPattern[searchPattern.Length - 1] == '*')
            {
                searchPattern = searchPattern.TrimStart('*').TrimEnd('*');

                if (searchPattern.Contains("*"))
                {
                    throw fex;
                }

                files = files.Where(f => f.Path.Replace(".gitignore", string.Empty).Contains(searchPattern));
            }
            else
            {
                int wildCardIndex = searchPattern.IndexOf('*');
                if (wildCardIndex != searchPattern.LastIndexOf('*'))
                {
                    throw fex;
                }

                searchPattern = searchPattern.ToLower();

                int endStart = wildCardIndex + 1;
                if (wildCardIndex < 0)
                {
                    wildCardIndex = searchPattern.Length;
                    endStart = 0;
                }

                string starts = searchPattern.Substring(0, Math.Max(0, wildCardIndex));
                string ends = searchPattern.Substring(Math.Min(endStart, searchPattern.Length));

                files = files.Where(t => t.Path.ToLower().StartsWith(starts)
                    && t.Path.Replace(".gitignore", string.Empty).ToLower().EndsWith(ends));
            }

            return files;
        }
    }
}
