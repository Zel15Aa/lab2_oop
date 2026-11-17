using System.Xml;

namespace LabXml.Strategies;

/// SAX-подібний аналіз: послідовне читання через XmlReader (forward-only).
public class SaxStrategy : ISearchStrategy
{
    public string Name => "SAX API";

    public List<Dictionary<string, string>> Search(string path, Dictionary<string, string> criteria)
    {
        var result = new List<Dictionary<string, string>>();
        Dictionary<string, string>? researcher = null;

        using var reader = XmlReader.Create(path);
        while (reader.Read())
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                if (reader.Name == "Researcher")
                    researcher = ReadAttributes(reader);
                else if (reader.Name == "Work" && researcher != null)
                {
                    var rec = new Dictionary<string, string>(researcher);
                    foreach (var a in ReadAttributes(reader)) rec[a.Key] = a.Value;
                    if (Criteria.Match(rec, criteria)) result.Add(rec);
                }
            }
            else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "Researcher")
                researcher = null;
        }
        return result;
    }

    private static Dictionary<string, string> ReadAttributes(XmlReader r)
    {
        var d = new Dictionary<string, string>();
        if (r.HasAttributes)
        {
            while (r.MoveToNextAttribute()) d[r.Name] = r.Value;
            r.MoveToElement();
        }
        return d;
    }
}
