public static class NameGenerator
{
    private static readonly List<string> _maleNames;
    private static readonly List<string> _femaleNames;
    private static readonly List<string> _lastNames;

    static NameGenerator()
    {
        _maleNames = LoadNamesByGender("Data/common-forenames-by-country.csv", "Romanized Name", "Gender", "M");
        _femaleNames = LoadNamesByGender("Data/common-forenames-by-country.csv", "Romanized Name", "Gender", "F");
        _lastNames = LoadNames("Data/common-surnames-by-country.csv", "Romanized Name");
    }

    private static List<string> LoadNamesByGender(string filePath, string nameColumn, string genderColumn, string targetGender)
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
            int nameIndex = Array.IndexOf(header, nameColumn);
            int genderIndex = Array.IndexOf(header, genderColumn);

            if (nameIndex == -1 || genderIndex == -1)
            {
                return new List<string> { "Default" };
            }

            for (int i = 1; i < lines.Length; i++)
            {
                var columns = lines[i].Split(',');
                if (columns.Length > Math.Max(nameIndex, genderIndex) && 
                    !string.IsNullOrWhiteSpace(columns[nameIndex]) &&
                    columns[genderIndex] == targetGender)
                {
                    names.Add(columns[nameIndex]);
                }
            }
            return names.Count > 0 ? names : new List<string> { "Default" };
        }
        catch (Exception)
        {
            return new List<string> { "Default" };
        }
    }

    private static List<string> LoadNames(string filePath, string columnName)
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

    public static string GenerateFirstName(string gender)
    {
        var firstNameList = gender == "Male" ? _maleNames : _femaleNames;
        
        if (firstNameList.Count == 0)
        {
            return "Default";
        }

        return firstNameList[Random.Shared.Next(firstNameList.Count)];
    }

    public static string GenerateLastName()
    {
        if (_lastNames.Count == 0)
        {
            return "Name";
        }
        return _lastNames[Random.Shared.Next(_lastNames.Count)];
    }
}