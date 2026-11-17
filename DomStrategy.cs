using System.Xml;

namespace LabXml.Strategies;

/// <summary>DOM-аналіз: документ повністю завантажується в пам'ять (XmlDocument).</summary>
public class DomStrategy : ISearchStrategy
{
    public string Name => "DOM API";

    public List<Dictionary<string, string>> Search(string path, Dictionary<string, string> criteria)
    {
        var result = new List<Dictionary<string, string>>();
        var doc = new XmlDocument();
        doc.Load(path);

        foreach (XmlNode researcher in doc.SelectNodes("//Researcher")!)
        {
            var rAttrs = Attrs(researcher);
            foreach (XmlNode work in researcher.SelectNodes("Work")!)
            {
                var rec = new Dictionary<string, string>(rAttrs);
                foreach (var a in Attrs(work)) rec[a.Key] = a.Value;
                if (Criteria.Match(rec, criteria)) result.Add(rec);
            }
        }
        return result;
    }

    private static Dictionary<string, string> Attrs(XmlNode n) =>
        n.Attributes!.Cast<XmlAttribute>().ToDictionary(a => a.Name, a => a.Value);
}
