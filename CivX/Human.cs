public class Human
{
    public string FullName => $"{FirstName} {LastName}";
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Gender { get; set; }
    public int Age { get; set; }
    public int BirthYear { get; set; } = DateTime.Now.Year;
    public int DeathYear { get; set; }
    public bool IsAlive { get; set; } = true;
    public Human[]? Parents { get; set; }
    public Human? Spouse { get; set; } = null;
    public string? Job { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public Human[]? Children { get; set; }
    public int Balance { get; set; }
    public Stats Stats { get; set; } = new();

    public Human()
    {
        Gender = Random.Shared.Next(0, 2) == 0 ? "Male" : "Female";
        FirstName = NameGenerator.GenerateFirstName(Gender);
        LastName = NameGenerator.GenerateLastName();
    }

    public void AgeUp(int currentYear)
    {
        Age++;

        const double maxAge = 130.0;
        const double riskPower = 5.0;

        double chanceOfDeath = Math.Pow(Age / maxAge, riskPower);

        double roll = Random.Shared.NextDouble();

        if (roll < chanceOfDeath)
        {
            IsAlive = false;
            DeathYear = currentYear;
        }
    }
}