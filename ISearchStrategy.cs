namespace LabXml.Strategies;

// <summary>Strategy: спільний інтерфейс усіх способів аналізу XML.
/// Результат - "плоскі" записи: атрибути науковця + атрибути його роботи.</summary>
public interface ISearchStrategy
{
    string Name { get; }
    List<Dictionary<string, string>> Search(string path, Dictionary<string, string> criteria);
}

public static class Criteria
{
    // Запис підходить, якщо містить ВСІ задані атрибути з потрібними значеннями
    public static bool Match(Dictionary<string, string> rec, Dictionary<string, string> criteria) =>
        criteria.All(c => rec.TryGetValue(c.Key, out var v) && v == c.Value);
}
