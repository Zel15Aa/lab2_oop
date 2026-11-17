using System.Xml.Linq;

namespace LabXml.Strategies;

/// <summary>LINQ to XML (XDocument).</summary>
public class LinqStrategy : ISearchStrategy
{
    public string Name => "LINQ to XML";

    public List<Dictionary<string, string>> Search(string path, Dictionary<string, string> criteria)
    {
        var doc = XDocument.Load(path);

        return (from r in doc.Descendants("Researcher")
                from w in r.Elements("Work")
                let rec = r.Attributes().Concat(w.Attributes())
                           .ToDictionary(a => a.Name.LocalName, a => a.Value)
                where Criteria.Match(rec, criteria)
                select rec).ToList();
    }
}
