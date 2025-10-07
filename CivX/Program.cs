using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

/*
    TODO-
    Set up marrying.
    Set up kids. 
    Add logging
    Set up hour passed. 
    Set up savable settings.
    Set up jobs.
    Set up income.
*/

Console.Clear();

string savesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Saves");
Directory.CreateDirectory(savesDirectory);

var saveFiles = Directory.GetFiles(savesDirectory, "*.json");
SaveData? currentSave = null;
string currentSaveFilePath = "";

if (saveFiles.Length == 0)
{
    Console.WriteLine("No save files found.");
    currentSave = CreateNewWorld();
}
else
{
    Console.WriteLine("Please choose a save file to load, or create a new one:");
    for (int i = 0; i < saveFiles.Length; i++)
    {
        var saveData = LoadWorld(saveFiles[i]);
        if (saveData != null)
        {
            string displayName = Path.GetFileNameWithoutExtension(saveFiles[i]);
            Console.WriteLine($"[{i + 1}] {displayName} - Year: {saveData.CurrentYear}, Last Saved: {saveData.SaveTime}");
        }
    }
    Console.WriteLine("[n] Create a New World");

    while (currentSave == null)
    {
        Console.Write("Your choice: ");
        string? choice = Console.ReadLine();

        if (choice?.ToLower() == "n")
        {
            currentSave = CreateNewWorld();
        }
        else if (int.TryParse(choice, out int fileIndex) && fileIndex > 0 && fileIndex <= saveFiles.Length)
        {
            string selectedFile = saveFiles[fileIndex - 1];
            currentSaveFilePath = selectedFile;
            Console.WriteLine($"\nLoading save: {Path.GetFileName(selectedFile)}");
            currentSave = LoadWorld(selectedFile);
        }
        else
        {
            Console.WriteLine("Invalid choice. Please try again.");
        }
    }
}

if (currentSave == null)
{
    Console.WriteLine("Error loading save. Exiting...");
    Environment.Exit(0);
}

Console.WriteLine($"\nSave loaded. Game was last saved at: {currentSave.SaveTime}. ");
Console.WriteLine("Humans in this world:");
foreach (var human in currentSave.Humans)
{
    Console.WriteLine($"- {human.FullName}, Age: {human.Age}");
}

//Gameplay loop
bool firstRun = true;
while (true)
{
    Console.Clear();
    if (firstRun) Console.WriteLine("Welcome to CivX\n\n");
    Console.WriteLine($"Year: {currentSave.CurrentYear}");
    Console.WriteLine($"Humans: {currentSave.Humans.Count(h => h.IsAlive)}, Deaths: {currentSave.Humans.Count(h => !h.IsAlive)}");
    Console.WriteLine("What would you like to do? (Hit i to view commands)\n");
    string? command = Console.ReadLine();

    switch (command)
    {
        case "i":
            Console.WriteLine("\nAvailable Commands:");
            Console.WriteLine(" i - View commands");
            Console.WriteLine(" y - Move forward time one year");
            Console.WriteLine(" Y - Move forward x amount of time");
            Console.WriteLine(" s - Save");
            Console.WriteLine(" q - Quit");
            break;
        case "y":
            PassTime(1);
            Console.WriteLine("A year has passed.");
            break;
        case "Y":
            Console.Write("How many years do you want to pass?\n");
            if (int.TryParse(Console.ReadLine(), out int yearsToPass) && yearsToPass > 0)
            {
                PassTime(yearsToPass);
                Console.WriteLine($"{yearsToPass} years have passed.");
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a positive number.");
            }
            break;
        case "s":
            Console.WriteLine("Saving...");
            SaveGame(currentSave, currentSaveFilePath);
            Console.WriteLine("Saved.");
            break;
        case "q":
            Console.WriteLine("Exiting CivX. Goodbye!");
            return;
        default:
            Console.WriteLine("Unknown command. Hit 'i' to view all commands.");
            break;
    }
    Thread.Sleep(750);
}

void PassTime(int years)
{
    for (var i = 0; i < years; i++)
    {
        currentSave.CurrentYear++;
        foreach (var human in currentSave.Humans)
        {
            if (!human.IsAlive) continue;
            human.AgeUp(currentSave.CurrentYear);
            //If human meets another. Get married if they are attractive.
            if (human.Spouse == null)
            {
                var chance = Random.Shared.Next(0, 101);
                //20 percent chance they meet another human this year.
                if (chance <= 20)
                {
                    var potentialSpouse = currentSave.Humans[Random.Shared.Next(0, currentSave.Humans.Count)];
                    if (potentialSpouse.LastName == human.LastName) continue;
                    if (human.Stats.Attractiveness >= potentialSpouse.Stats.Attractiveness && potentialSpouse.Spouse == null)
                    {
                        human.Spouse = potentialSpouse;
                        potentialSpouse.Spouse = human;
                    }
                }
            }
            else
            {
                //If already married. Maybe have kids.
            }
        }
    }
}

SaveData CreateNewWorld()
{
    Console.Write("Enter a name for your new world: ");
    string? newSaveName = Console.ReadLine();

    while (string.IsNullOrWhiteSpace(newSaveName))
    {
        Console.Write("Save name cannot be empty. Please enter a name: ");
        newSaveName = Console.ReadLine();
    }

    if (!newSaveName.EndsWith(".json"))
    {
        newSaveName += ".json";
    }

    currentSaveFilePath = Path.Combine(savesDirectory, newSaveName);

    var initialHumans = new List<Human>();
    for (int i = 0; i < 10; i++)
    {
        initialHumans.Add(new Human { Age = 18 });
    }

    var newSave = new SaveData
    {
        Humans = initialHumans,
        CurrentYear = DateTime.Now.Year,
        SaveTime = DateTime.Now
    };

    SaveGame(newSave, currentSaveFilePath);
    Console.WriteLine($"New world saved as {newSaveName}");
    return newSave;
}

void SaveGame(SaveData data, string filePath)
{
    data.SaveTime = DateTime.Now;
    var options = new JsonSerializerOptions
    {
        ReferenceHandler = ReferenceHandler.Preserve,
        
        WriteIndented = true 
    };
    string jsonString = JsonSerializer.Serialize(data, options);
    File.WriteAllText(filePath, jsonString);
}

SaveData? LoadWorld(string filePath)
{
    string jsonString = File.ReadAllText(filePath);
    var options = new JsonSerializerOptions
    {
        ReferenceHandler = ReferenceHandler.Preserve
    };
    return JsonSerializer.Deserialize<SaveData>(jsonString, options);
}
