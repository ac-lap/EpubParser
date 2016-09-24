using EpubReader.Collections;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace EpubReader
{
  public class Epub
  {
    private static Regex _RefsRegex = new Regex("(?<prefix><\\w+[^>]*?href\\s*=\\s*(\"|'))(?<href>[^\"']*)(?<suffix>(\"|')[^>]*>)", Utils.REO_ci);
    private static Regex _ExternalLinksRegex = new Regex("^\\s*(http(s)?://|mailto:|ftp(s)?://)", Utils.REO_ci);
    private readonly Dictionary<string, string> _LinksMapping = new Dictionary<string, string>((IEqualityComparer<string>) StringComparer.CurrentCultureIgnoreCase);
    private const string _HtmlTemplate = "<!DOCTYPE html\r\n\t\t\t  PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">\r\n\t\t\t<html>\r\n\t\t\t   <head>\r\n\t\t\t\t  <title>{0}</title>\r\n\t\t\t\t  {1}\r\n\t\t\t   </head>\r\n\t\t\t   {2}\r\n\t\t\t\t  {3}\r\n\t\t\t   </body>\r\n\t\t\t</html>";
    private readonly ZipFile _EpubFile;
    private readonly string _ContentOpfPath;
    private string _TocFileName;
    private string _CurrentFileName;

    public string UUID { get; private set; }

    public List<string> ID { get; private set; }

    public List<string> Title { get; private set; }

    public List<string> Language { get; private set; }

    public Stream CoverImage { get; private set; }

    public List<string> Creator { get; private set; }

    public List<string> Description { get; private set; }

    public List<DateData> Date { get; private set; }

    public List<string> Publisher { get; private set; }

    public List<string> Contributer { get; private set; }

    public List<string> Type { get; private set; }

    public List<string> Format { get; private set; }

    public List<string> Subject { get; private set; }

    public List<string> Source { get; private set; }

    public List<string> Relation { get; private set; }

    public List<string> Coverage { get; private set; }

    public List<string> Rights { get; private set; }

    public OrderedDictionary Content { get; private set; }

    public OrderedDictionary ExtendedData { get; private set; }

    public List<NavPoint> TOC { get; private set; }

    public Epub(Stream epubStream)
    {
      this.ID = new List<string>();
      this.Title = new List<string>();
      this.Language = new List<string>();
      this.Creator = new List<string>();
      this.Description = new List<string>();
      this.Date = new List<DateData>();
      this.Publisher = new List<string>();
      this.Contributer = new List<string>();
      this.Type = new List<string>();
      this.Format = new List<string>();
      this.Subject = new List<string>();
      this.Source = new List<string>();
      this.Relation = new List<string>();
      this.Coverage = new List<string>();
      this.Rights = new List<string>();
      this.Content = new OrderedDictionary();
      this.ExtendedData = new OrderedDictionary();
      this.TOC = new List<NavPoint>();
      using (this._EpubFile = new ZipFile(epubStream))
      {
        string opfFilePath = Epub.GetOpfFilePath(this._EpubFile);
        if (string.IsNullOrEmpty(opfFilePath))
          throw new Exception("Invalid epub file.");
        Match match = Regex.Match(opfFilePath, "^.*/", Utils.REO_c);
        this._ContentOpfPath = match.Success ? match.Value : "";
        this.LoadEpubMetaDataFromOpfFile(opfFilePath);
        if (this._TocFileName == null)
          return;
        this.LoadTableOfContents();
      }
    }

    public string GetContentAsPlainText()
    {
      StringBuilder stringBuilder = new StringBuilder();
      int index = 0;
      for (int count = this.Content.Count; index < count; ++index)
        stringBuilder.Append(((ContentData) this.Content[index]).GetContentAsPlainText());
      return stringBuilder.ToString();
    }

    private string EmbedImages(string html)
    {
      return Regex.Replace(html, "(?<prefix><\\w+[^>]*?src\\s*=\\s*(\"|'))(?<src>[^\"']+)(?<suffix>(\"|')[^>]*>)", new MatchEvaluator(this.SrcEvaluator), Utils.REO_ci);
    }

    private string SrcEvaluator(Match match)
    {
      EpubReader.ExtendedData extendedData = this.ExtendedData[(object) Epub.GetTrimmedFileName(match.Groups["src"].Value, true)] as EpubReader.ExtendedData;
      if (extendedData == null)
        return match.Value;
      return match.Groups["prefix"].Value + "data:" + extendedData.MimeType + ";base64," + extendedData.Content + match.Groups["suffix"].Value;
    }

        private static string GetOpfFilePath(ZipFile epubFile)
        {
            Stream fileStream = null;
            foreach (ZipEntry theEntry in epubFile)
            {
                if (theEntry.Name.Equals("meta-inf/container.xml", StringComparison.CurrentCultureIgnoreCase))
                {
                    fileStream = epubFile.GetInputStream(theEntry);
                    break;
                }
            }

            if (fileStream != null)
            {
                XElement xelement1 = XElement.Load(fileStream);
                string text = new StreamReader(fileStream).ReadToEnd();
                XNamespace xnamespace1 = xelement1.Attribute((XName)"xmlns") != null ? (XNamespace)xelement1.Attribute((XName)"xmlns").Value : XNamespace.None;
                if (xnamespace1 != XNamespace.None)
                    return Enumerable.FirstOrDefault<XElement>(xelement1.Descendants(xnamespace1 + "rootfile"), (Func<XElement, bool>)(p =>
                    {
                        if (p.Attribute((XName)"media-type") != null)
                            return p.Attribute((XName)"media-type").Value.Equals("application/oebps-package+xml", StringComparison.CurrentCultureIgnoreCase);
                        return false;
                    })).Attribute((XName)"full-path").Value;
                XDocument xdocument = XDocument.Parse(text);
                xdocument.Root.Add((object)new XAttribute((XName)"xmlns", (object)"urn:oasis:names:tc:opendocument:xmlns:container"));
                Extensions.Remove(xdocument.Root.Attributes(XNamespace.Xmlns + "odfc"));
                XElement xelement2 = XElement.Parse(xdocument.ToString());
                XNamespace xnamespace2 = xelement2.Attribute((XName)"xmlns") != null ? (XNamespace)xelement2.Attribute((XName)"xmlns").Value : XNamespace.None;
                if (xnamespace2 != XNamespace.None)
                    return Enumerable.FirstOrDefault<XElement>(xelement2.Descendants(xnamespace2 + "rootfile"), (Func<XElement, bool>)(p =>
                    {
                        if (p.Attribute((XName)"media-type") != null)
                            return p.Attribute((XName)"media-type").Value.Equals("application/oebps-package+xml", StringComparison.CurrentCultureIgnoreCase);
                        return false;
                    })).Attribute((XName)"full-path").Value;
            }
            return (string)null;
        }

        private void LoadEpubMetaDataFromOpfFile(string opfFilePath)
        {
            XElement contentOpf;

            Stream fileStream = null;
            foreach (ZipEntry theEntry in this._EpubFile)
            {
                if (theEntry.Name.Equals(opfFilePath, StringComparison.CurrentCultureIgnoreCase))
                {
                    fileStream = this._EpubFile.GetInputStream(theEntry);
                    break;
                }
            }

            if (fileStream == null)
                throw new Exception("Invalid epub file.");

            contentOpf = XElement.Load(fileStream);

            XNamespace xNamespace = contentOpf.Attribute((XName)"xmlns") != null ? (XNamespace)contentOpf.Attribute((XName)"xmlns").Value : XNamespace.None;
            string str = contentOpf.Attribute((XName)"unique-identifier").Value;
            foreach (XElement xelement in Enumerable.Where<XElement>(Extensions.Elements<XElement>(contentOpf.Elements(xNamespace + "metadata")), (Func<XElement, bool>)(e => e.Value.Trim() != string.Empty)))
            {
                switch (xelement.Name.LocalName)
                {
                    case "title":
                        this.Title.Add(xelement.Value);
                        continue;
                    case "creator":
                        this.Creator.Add(xelement.Value);
                        continue;
                    case "date":
                        XAttribute xattribute = Enumerable.FirstOrDefault<XAttribute>(xelement.Attributes(), (Func<XAttribute, bool>)(a => a.Name.LocalName == "event"));
                        if (xattribute != null)
                        {
                            this.Date.Add(new DateData(xattribute.Value, xelement.Value));
                            continue;
                        }
                        continue;
                    case "publisher":
                        this.Publisher.Add(xelement.Value);
                        continue;
                    case "subject":
                        this.Subject.Add(xelement.Value);
                        continue;
                    case "source":
                        this.Source.Add(xelement.Value);
                        continue;
                    case "rights":
                        this.Rights.Add(xelement.Value);
                        continue;
                    case "description":
                        this.Description.Add(xelement.Value);
                        continue;
                    case "contributor":
                        this.Contributer.Add(xelement.Value);
                        continue;
                    case "type":
                        this.Type.Add(xelement.Value);
                        continue;
                    case "format":
                        this.Format.Add(xelement.Value);
                        continue;
                    case "identifier":
                        this.ID.Add(xelement.Value);
                        continue;
                    case "language":
                        this.Language.Add(xelement.Value);
                        continue;
                    case "relation":
                        this.Relation.Add(xelement.Value);
                        continue;
                    case "coverage":
                        this.Coverage.Add(xelement.Value);
                        continue;
                    default:
                        continue;
                }
            }
            this.LoadManifestSectionFromOpfFile(contentOpf, xNamespace);
            //this.CoverImage = this.getCoverImage(contentOpf, xNamespace);
        }

    private Stream getCoverImage(XElement contentOpf, XNamespace xNamespace)
    {
      IEnumerable<string> source = Enumerable.Select<XElement, string>(Enumerable.Where<XElement>(Extensions.Elements<XElement>(contentOpf.Elements(xNamespace + "metadata"), xNamespace + "meta"), (Func<XElement, bool>) (metaNodes => metaNodes.Attribute((XName) "name").Value.Equals("cover", StringComparison.CurrentCultureIgnoreCase))), (Func<XElement, string>) (metaNodes => metaNodes.Attribute((XName) "content").Value));
      string meta_CoverID = string.Empty;
      try
      {
        meta_CoverID = Enumerable.FirstOrDefault<string>(source);
      }
      catch (Exception ex)
      {
      }
      if (!string.IsNullOrEmpty(meta_CoverID))
      {
        XElement xelement = Enumerable.FirstOrDefault<XElement>(Extensions.Elements<XElement>(contentOpf.Elements(xNamespace + "manifest")), (Func<XElement, bool>) (e =>
        {
          if (e.Attribute((XName) "id").Value.Equals(meta_CoverID))
            return e.Attribute((XName) "media-type").Value.StartsWith("image", StringComparison.CurrentCultureIgnoreCase);
          return false;
        }));
        if (xelement != null)
          return (this.ExtendedData[(object) Epub.GetTrimmedFileName(xelement.Attribute((XName) "href").Value, true)] as EpubReader.ExtendedData).GetContentAsStream();
      }
      return (Stream) null;
    }

        private void LoadManifestSectionFromOpfFile(XElement contentOpf, XNamespace xNamespace)
        {
            HashSet<string> alreadyProcessedFiles = new HashSet<string>((IEqualityComparer<string>)StringComparer.CurrentCultureIgnoreCase);
            foreach (XElement xelement1 in Extensions.Elements<XElement>(contentOpf.Elements(xNamespace + "spine")))
            {
                XElement spinElement = xelement1;
                XElement xelement2 = Enumerable.FirstOrDefault<XElement>(Extensions.Elements<XElement>(contentOpf.Elements(xNamespace + "manifest")), (Func<XElement, bool>)(e => e.Attribute((XName)"id").Value == spinElement.Attribute((XName)"idref").Value));
                if (xelement2 == null)
                    throw new Exception("Invalid epub file.");
                if (xelement2 != null)
                {
                    string fileName = WebUtility.UrlDecode(xelement2.Attribute((XName)"href").Value);
                    Stream fileStream = null;
                    foreach (ZipEntry theEntry in this._EpubFile)
                    {
                        if (theEntry.Name.Equals(this._ContentOpfPath + fileName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            fileStream = this._EpubFile.GetInputStream(theEntry);
                            break;
                        }
                    }
                    if (fileStream == null)
                        throw new Exception("Invalid epub file.");
                    if (!this.Content.Contains((object)fileName))
                        this.Content.Add((object)fileName, (object)new ContentData(fileName, fileStream));

                    if (!alreadyProcessedFiles.Contains(spinElement.Attribute((XName)"idref").Value))
                        alreadyProcessedFiles.Add(spinElement.Attribute((XName)"idref").Value);
                }
            }
            foreach (XElement xelement in Enumerable.Where<XElement>(Extensions.Elements<XElement>(contentOpf.Elements(xNamespace + "manifest")), (Func<XElement, bool>)(e => !alreadyProcessedFiles.Contains(e.Attribute((XName)"id").Value))))
            {
                XAttribute xattribute = xelement.Attribute((XName)"href");
                if (xattribute != null)
                {
                    string fileName = xattribute.Value;
                    Stream fileStream = null;
                    foreach (ZipEntry theEntry in this._EpubFile)
                    {
                        if (theEntry.Name.Equals(this._ContentOpfPath + fileName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            fileStream = this._EpubFile.GetInputStream(theEntry);
                            break;
                        }
                    }
                    if (fileStream != null)
                    {
                        string trimmedFileName = Epub.GetTrimmedFileName(fileName, true);
                        if (!this.ExtendedData.Contains((object)trimmedFileName))
                            this.ExtendedData.Add((object)trimmedFileName, (object)new EpubReader.ExtendedData(fileName, xelement.Attribute((XName)"media-type").Value, fileStream));
                        if (string.Equals(xelement.Attribute((XName)"media-type").Value, "application/x-dtbncx+xml", StringComparison.CurrentCultureIgnoreCase))
                            this._TocFileName = xelement.Attribute((XName)"href").Value;
                    }
                }
            }
        }

    private static void CollectReplacementLinks(Dictionary<string, string> linksMapping, string fileName, string text)
    {
      foreach (Match match in Epub._RefsRegex.Matches(text))
      {
        if (!Epub._ExternalLinksRegex.IsMatch(match.Groups["href"].Value))
        {
          string index = (Epub.GetTrimmedFileName(match.Groups["href"].Value, true) ?? Epub.GetTrimmedFileName(fileName, true)) + Epub.GetAnchorValue(match.Groups["href"].Value);
          linksMapping[index] = Epub.GetNormalizedSrc(match.Groups["href"].Value);
        }
      }
    }

    private string NormalizeRefs(string text)
    {
      if (text == null)
        return (string) null;
      text = Epub._RefsRegex.Replace(text, new MatchEvaluator(Epub.RefsEvaluator));
      text = Regex.Replace(text, "(?<prefix>\\bid\\s*=\\s*(\"|'))(?<id>[^\"']+)", new MatchEvaluator(this.IdsEvaluator), Utils.REO_ci);
      return text;
    }

    private static string RefsEvaluator(Match match)
    {
      if (Epub._ExternalLinksRegex.IsMatch(match.Groups["href"].Value))
        return match.Value.Insert(match.Value.Length - 2, "target=\"_blank\"");
      return match.Groups["prefix"].Value + Epub.GetNormalizedSrc(match.Groups["href"].Value) + match.Groups["suffix"].Value;
    }

    private static string GetAnchorValue(string fileName)
    {
      Match match = Regex.Match(fileName, "\\#(?<anchor>.+)", Utils.REO_c);
      if (!match.Success)
        return "";
      return "#" + match.Groups["anchor"].Value;
    }

    private string IdsEvaluator(Match match)
    {
      string key = Epub.GetTrimmedFileName(this._CurrentFileName, true) + "#" + match.Groups["id"].Value;
      if (!this._LinksMapping.ContainsKey(key))
        return match.Value;
      return match.Groups["prefix"].Value + this._LinksMapping[key].Replace("#", "");
    }

    private void LoadTableOfContents()
    {
      EpubReader.ExtendedData extendedData = this.ExtendedData[(object) this._TocFileName] as EpubReader.ExtendedData;
      if (extendedData == null)
        return;
      XElement xelement1 = XElement.Parse(extendedData.Content);
      XNamespace nameSpace1 = xelement1.Attribute((XName) "xmlns") != null ? (XNamespace) xelement1.Attribute((XName) "xmlns").Value : XNamespace.None;
      if (nameSpace1 != XNamespace.None)
      {
        this.TOC = this.GetNavigationChildren(xelement1.Element(nameSpace1 + "navMap").Elements(nameSpace1 + "navPoint"), nameSpace1);
      }
      else
      {
        XDocument xdocument = XDocument.Parse(extendedData.Content);
        xdocument.Root.Add((object) new XAttribute((XName) "xmlns", (object) "http://www.daisy.org/z3986/2005/ncx/"));
        Extensions.Remove(xdocument.Root.Attributes(XNamespace.Xmlns + "ncx"));
        XElement xelement2 = XElement.Parse(xdocument.ToString());
        XNamespace nameSpace2 = xelement2.Attribute((XName) "xmlns") != null ? (XNamespace) xelement2.Attribute((XName) "xmlns").Value : XNamespace.None;
        if (!(nameSpace2 != XNamespace.None))
          return;
        this.TOC = this.GetNavigationChildren(xelement2.Element(nameSpace2 + "navMap").Elements(nameSpace2 + "navPoint"), nameSpace2);
      }
    }

    private List<NavPoint> GetNavigationChildren(IEnumerable<XElement> elements, XNamespace nameSpace)
    {
      List<NavPoint> list = new List<NavPoint>(Enumerable.Count<XElement>(elements));
      if (!Enumerable.Any<XElement>(elements))
        return list;
      list.AddRange(Enumerable.Select<XElement, NavPoint>(elements, (Func<XElement, NavPoint>) (navPoint => new NavPoint(navPoint.Attribute((XName) "id").Value, navPoint.Element(nameSpace + "navLabel").Element(nameSpace + "text").Value, WebUtility.UrlDecode(navPoint.Element(nameSpace + "content").Attribute((XName) "src").Value), int.Parse(navPoint.Attribute((XName) "playOrder").Value), this.Content[(object) Epub.NormalizeFileName(WebUtility.UrlDecode(navPoint.Element(nameSpace + "content").Attribute((XName) "src").Value))] as ContentData, this.GetNavigationChildren(navPoint.Elements(nameSpace + "navPoint"), nameSpace)))));
      return list;
    }

    private static string NormalizeFileName(string fileName)
    {
      if (fileName != null)
        return Regex.Replace(fileName, "\\#.*$", "", Utils.REO_c);
      return (string) null;
    }

    private string GetTocHtml(List<NavPoint> navPoints)
    {
      if (navPoints == null || navPoints.Count == 0)
        return "";
      StringBuilder stringBuilder = new StringBuilder("<ul>");
      foreach (NavPoint navPoint in navPoints)
      {
        string normalizedSrc = Epub.GetNormalizedSrc(navPoint.Source);
        stringBuilder.AppendFormat("<li><a href=\"{0}\">{1}</a>", new object[2]
        {
          (object) normalizedSrc,
          (object) navPoint.Title
        });
        this._LinksMapping[Epub.GetTrimmedFileName(navPoint.Source, false)] = normalizedSrc;
        stringBuilder.AppendFormat("{0}</li>", new object[1]
        {
          (object) this.GetTocHtml(navPoint.Children)
        });
      }
      stringBuilder.Append("</ul>");
      return stringBuilder.ToString();
    }

    private static string GetNormalizedSrc(string originalSrc)
    {
      string trimmedFileName = Epub.GetTrimmedFileName(originalSrc, false);
      if (trimmedFileName == null)
        return (string) null;
      return "#" + trimmedFileName.Replace('.', '_').Replace('#', '_');
    }

    private static string GetTrimmedFileName(string fileName, bool removeAnchor)
    {
      Match match = Regex.Match(fileName, "/?(?<fileName>[^/]+)$", Utils.REO_c);
      if (!match.Success)
        return (string) null;
      if (!removeAnchor)
        return match.Groups["fileName"].Value;
      string str = Regex.Replace(match.Groups["fileName"].Value, "\\#.*", "", Utils.REO_c);
      if (!(str.Trim() != string.Empty))
        return (string) null;
      return str;
    }

    private string EmbedCssData(string head)
    {
      return Regex.Replace(head, "<link\\s+[^>]*?(href\\s*=\\s*(\"|')(?<href>[^\"']+)(\"|')[^>]*?|type\\s*=\\s*(\"|')text/css(\"|')[^>]*?){2}[^>]*?/>", new MatchEvaluator(this.CssEvaluator), Utils.REO_ci);
    }

    private string CssEvaluator(Match match)
    {
      EpubReader.ExtendedData extendedData = this.ExtendedData[(object) Epub.GetTrimmedFileName(match.Groups["href"].Value, true)] as EpubReader.ExtendedData;
      if (extendedData == null)
        return match.Value;
      return string.Format("<style type=\"text/css\">{0}</style>", (object) extendedData.Content);
    }
  }
}
