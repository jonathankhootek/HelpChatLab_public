using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpChat.Utils
{
    internal static class FileUtils
    {
        internal static string GetFileContents(string filePath)
        {
            return string.Join(Environment.NewLine, File.ReadLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath)).Where(line => !line.TrimStart().StartsWith("#")));
        }
    }
}
