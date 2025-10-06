public class Human
{
    public string? Name { get; set; }
    public int Age { get; set; }
    public int BirthYear { get; set; } = DateTime.Now.Year;
    public int DeathYear { get; set; }
    public bool IsAlive { get; set; } = true;
    public Human[]? Parents { get; set; }
    public string? Job { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public Human[]? Children { get; set; }
    public int Balance { get; set; }

    public void AgeUp(int currentYear)
    {
        if (!IsAlive)
        {
            return;
        }

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