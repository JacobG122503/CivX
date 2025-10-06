public class NameGenerator
{
    private readonly List<string> _firstNames;
    private readonly List<string> _lastNames;

    public NameGenerator()
    {
        _firstNames = LoadNames("Data/common-forenames-by-country.csv", "Romanized Name");
        _lastNames = LoadNames("Data/common-surnames-by-country.csv", "Romanized Name");
    }

    private List<string> LoadNames(string filePath, string columnName)
    {
        var names = new List<string>();
        try
        {
            var lines = File.ReadAllLines(filePath);
            if (lines.Length < 2)
            {
                return new List<string> { "Default" };
            }

            var header = lines[0].Split(',');
            int nameIndex = Array.IndexOf(header, columnName);

            if (nameIndex == -1)
            {
                return new List<string> { "Default" };
            }

            for (int i = 1; i < lines.Length; i++)
            {
                var columns = lines[i].Split(',');
                if (columns.Length > nameIndex && !string.IsNullOrWhiteSpace(columns[nameIndex]))
                {
                    names.Add(columns[nameIndex]);
                }
            }
            return names;
        }
        catch (Exception)
        {
            return new List<string> { "Default" };
        }
    }

    public (string, string) GenerateRandomName()
    {
        if (_firstNames.Count == 0 || _lastNames.Count == 0)
        {
            return ("Default", "Name");
        }

        string firstName = _firstNames[Random.Shared.Next(_firstNames.Count)];
        string lastName = _lastNames[Random.Shared.Next(_lastNames.Count)];

        return (firstName, lastName);
    }
}