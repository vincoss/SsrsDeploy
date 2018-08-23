using System;
using System.Diagnostics;
using System.IO;
using System.Text;


namespace SsrsDeploy.Engine
{
    [DebuggerDisplay("Name: {Name}, FullName: {FullName}, SortOrder: {SortOrder}, ContentHash: {ContentHash}")]
    public class ReportItem
    {
        public ReportItem(string name, string content)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }
            if (string.IsNullOrEmpty(content))
            {
                throw new ArgumentNullException(nameof(content));
            }
            FullName = name;
            Name = GetFileNameFromResourceName(FullName);
            Content = content;
            ContentHash = Extensions.GetKnuthHash(content);
            SortOrder = GetSortOrder(name);
        }

        public static int GetSortOrder(string name)
        {
            if(string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }
            
            if(name.EndsWith(".rds"))
            {
                return 0;
            }
            if (name.EndsWith(".rsd"))
            {
                return 1;
            }
            if (name.EndsWith(".rdl"))
            {
                return 2;
            }
            return 3;
        }

        public static ReportItem FromStream(string name, Stream stream)
        {
            return FromStream(name, stream, Encoding.Default);
        }

        public static ReportItem FromStream(string name, Stream stream, Encoding encoding)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }
            using (var resourceStreamReader = new StreamReader(stream, encoding, true))
            {
                var content = resourceStreamReader.ReadToEnd();
                return new ReportItem(name, content);
            }
        }

        public static string GetFileNameFromResourceName(string resourceName)
        {
            if (resourceName == null)
            {
                throw new ArgumentNullException(nameof(resourceName));
            }

            var fileName = resourceName;
            var extension = Path.GetExtension(resourceName);

            if (extension != null && extension.Length > 0)
            {
                fileName = fileName.Substring(0, fileName.IndexOf(extension, StringComparison.Ordinal));
            }

            var fileNameIndex = fileName.LastIndexOf('.');

            if (fileNameIndex >= 0)
            {
                var start = fileNameIndex + 1;
                fileName = fileName.Substring(start, fileName.Length - start);
            }
            return fileName;
        }

        public string Name { get; private set; }

        public string FullName { get; private set; }

        public string Content { get; private set; }

        public ulong ContentHash { get; private set; }

        public int SortOrder { get; set; }

    }
}
