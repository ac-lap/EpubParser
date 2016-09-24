using System;
using System.IO;

namespace EpubReader
{
  public class ExtendedData
  {
    private readonly Stream _ExtendedZipEntry;

    public string FileName { get; private set; }

    public string Content
    {
      get
      {
        if (!this.IsText)
          return (string) null;
        using (StreamReader streamReader = new StreamReader(this._ExtendedZipEntry))
          return streamReader.ReadToEnd();
      }
    }

    public bool IsText
    {
      get
      {
        if (!Path.GetExtension(this.FileName).Equals(".ncx", StringComparison.CurrentCultureIgnoreCase))
          return Path.GetExtension(this.FileName).Equals(".css", StringComparison.CurrentCultureIgnoreCase);
        return true;
      }
    }

    public string MimeType { get; private set; }

    public ExtendedData(string fileName, string mimeType, Stream extendedZipEntry)
    {
      this.FileName = fileName;
      this.MimeType = mimeType;
      this._ExtendedZipEntry = extendedZipEntry;
    }

    public Stream GetContentAsStream()
    {
      return this._ExtendedZipEntry;
    }
  }
}
