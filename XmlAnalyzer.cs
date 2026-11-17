using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using LabXml.Strategies;

namespace LabXml.Services;

/// Context патерну Strategy + допоміжні операції (підгрузка даних, XSLT).
public class XmlAnalyzer
{
    private ISearchStrategy _strategy;
    public XmlAnalyzer(ISearchStrategy strategy) => _strategy = strategy;
    public void SetStrategy(ISearchStrategy s) => _strategy = s;

    public List<Dictionary<string, string>> Search(string path, Dictionary<string, string> criteria) =>
        _strategy.Search(path, criteria);

    /// "Підгрузка даних": які атрибути і які їх значення реально є у файлі.
    public static SortedDictionary<string, SortedSet<string>> LoadAttributeValues(string path)
    {
        var doc = XDocument.Load(path);
        var map = new SortedDictionary<string, SortedSet<string>>();
        foreach (var el in doc.Descendants().Where(e => e.Name == "Researcher" || e.Name == "Work"))
            foreach (var a in el.Attributes())
            {
                if (!map.TryGetValue(a.Name.LocalName, out var set))
                    map[a.Name.LocalName] = set = new SortedSet<string>();
                set.Add(a.Value);
            }
        return map;
    }

    /// Трансформація результатів у HTML за допомогою XSL.
    public static async Task<string> ToHtmlAsync(List<Dictionary<string, string>> records)
    {
        var doc = new XDocument(new XElement("Results",
            records.Select(r => new XElement("Record", r.Select(kv => new XAttribute(kv.Key, kv.Value))))));

        using var xslStream = await FileSystem.OpenAppPackageFileAsync("results.xsl");
        using var xslReader = XmlReader.Create(xslStream);
        var xslt = new XslCompiledTransform();
        xslt.Load(xslReader);

        var path = Path.Combine(FileSystem.CacheDirectory, "result.html");
        using var src = doc.CreateReader();
        using var writer = XmlWriter.Create(path, xslt.OutputSettings);
        xslt.Transform(src, writer);
        return path;
    }
}
