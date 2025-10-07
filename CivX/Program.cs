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

//Change these to tweak world
const int CHANCE_OF_MEETING = 20;
const int WANT_BABY_DIFF = 35;

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
        var newBabys = new List<Human>();

        foreach (var human in currentSave.Humans)
        {
            if (!human.IsAlive) continue;
            human.AgeUp(currentSave.CurrentYear);
            //If human meets another. Get married if they are attractive.
            if (human.Spouse == null && human.Age >= 18)
            {
                var chance = Random.Shared.Next(0, 101);
                //20 percent chance they meet another human this year.
                if (chance <= CHANCE_OF_MEETING)
                {
                    //Might want to remove continue idk
                    var eligibleSpouses = currentSave.Humans.Where(h => h.IsAlive && h != human && h.Spouse == null).ToList();
                    if (eligibleSpouses.Count == 0) continue;

                    var potentialSpouse = eligibleSpouses[Random.Shared.Next(0, eligibleSpouses.Count)];
                    
                    if (potentialSpouse.LastName == human.LastName || potentialSpouse.Age < 18) continue;
                    if (human.Stats.Attractiveness >= potentialSpouse.Stats.Attractiveness)
                    {
                        human.Spouse = potentialSpouse;
                        potentialSpouse.Spouse = human;
                        if (human.Gender == "Male") human.Spouse.LastName = human.LastName;
                        else human.LastName = human.Spouse.LastName;
                    }
                }
            }
            else if (human.Spouse != null && human.Age >= 18 && human.Spouse.IsAlive)
            {
                //If already married. Maybe have kids.
                //If they BabyFever stat has a difference of WANT_BABY_DIFF or above, they choose never to have kids.
                //Then if they want kids, both the spouse and human must succeed in a roll to have kids.
                //If success, minus 20 from BabyFever for both.
                if (Math.Abs(human.Stats.BabyFever - human.Spouse.Stats.BabyFever) >= WANT_BABY_DIFF) continue;
                if (Random.Shared.Next(0, 101) >= human.Stats.BabyFever && Random.Shared.Next(0, 101) >= human.Spouse.Stats.BabyFever)
                {
                    human.Stats.BabyFever -= 20;
                    human.Spouse.Stats.BabyFever -= 20;
                    //Birth kid.
                    var baby = new Human();
                    baby.LastName = human.LastName;
                    human.Children.Add(baby);
                    human.Spouse.Children.Add(baby);
                    baby.Parents.Add(human);
                    baby.Parents.Add(human.Spouse);
                    baby.BirthYear = currentSave.CurrentYear;
                    newBabys.Add(baby);
                }
            }
        }
        currentSave.Humans.AddRange(newBabys);
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

    //Temp add random humans
    var initialHumans = new List<Human>();
    for (int i = 0; i < 100; i++)
    {
        initialHumans.Add(new Human { Age = 18, BirthYear = DateTime.Now.Year });
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
        WriteIndented = true,
    };
    string jsonString = JsonSerializer.Serialize(data, options);
    File.WriteAllText(filePath, jsonString);
}

SaveData? LoadWorld(string filePath)
{
    string jsonString = File.ReadAllText(filePath);
    var options = new JsonSerializerOptions
    {
        ReferenceHandler = ReferenceHandler.Preserve,
    };
    return JsonSerializer.Deserialize<SaveData>(jsonString, options);
}
