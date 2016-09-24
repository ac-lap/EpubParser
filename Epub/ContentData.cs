using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace EpubReader
{
    public class ContentData
    {
        private static Regex regex = new Regex("<body[^>]*>.+</body>", Utils.REO_csi);

        public string FileName { get; private set; }

        public string Content { get; private set; }

        public ContentData(string fileName, Stream zipEntry)
        {
            this.FileName = fileName;
            using (StreamReader streamReader = new StreamReader(zipEntry))
                this.Content = streamReader.ReadToEnd();
        }

        public string GetContentAsPlainText()
        {
            Match match = ContentData.regex.Match(this.Content);
            if (!match.Success)
                return "";
            return Utils.ClearText(match.Value);
        }

        public Tuple<string, List<byte>> GetContentAsFormattedText()
        {
            Match match = ContentData.regex.Match(this.Content);
            if (!match.Success)
                return null;
            return Utils.ForamttedText(match.Value);
        }
    }
}
