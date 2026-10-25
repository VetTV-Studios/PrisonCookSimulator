using System.Text.Json;

namespace PrisonCookSimulator;

public sealed class TrusteeDto
{
    public string Name { get; set; } = "";
    public string Note { get; set; } = "";
    public Station Best { get; set; }
    public int Skill { get; set; }
    public int Mood { get; set; } = 70;
}

public sealed class ContractDto
{
    public int Week { get; set; } = 1;
    public int Day { get; set; } = 1;
    public int Budget { get; set; }
    public int Hygiene { get; set; }
    public int Morale { get; set; }
    public int Favor { get; set; }
    public int Headcount { get; set; }
    public int ContractStars { get; set; }
    public int DaysPlayed { get; set; }
    public string MenuA { get; set; } = "";
    public string MenuB { get; set; } = "";
    public Dictionary<string, int> Pantry { get; set; } = new();
    public List<string> Owned { get; set; } = new();
    public List<TrusteeDto> Crew { get; set; } = new();
}

public enum Difficulty { Easy, Normal, Hard }

public sealed class SettingsDto
{
    public double MusicVolume { get; set; } = 0.5;
    public double SfxVolume { get; set; } = 0.8;
    public bool Muted { get; set; }
    public Difficulty Difficulty { get; set; } = Difficulty.Normal;
    public bool Fullscreen { get; set; }

    /// <summary>How fast the ticket clock drains relative to Normal.</summary>
    public double ClockSpeed => Difficulty switch
    {
        Difficulty.Easy => 0.75,
        Difficulty.Hard => 1.3,
        _ => 1.0,
    };
}

public sealed class SaveData
{
    public int Version { get; set; } = 2;
    public int BestRush { get; set; }
    public SettingsDto Settings { get; set; } = new();
    public ContractDto? Contract { get; set; }
}

/// <summary>
/// JSON save in %LocalAppData%\VetTV\PrisonCookSimulator\save.json.
/// Unpackaged: real path. Packaged (MSIX): Windows redirects it into the package's LocalCache, which still works.
/// </summary>
public static class SaveStore
{
    private static readonly JsonSerializerOptions Opts = new() { WriteIndented = true };

    public static string Folder => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VetTV", "PrisonCookSimulator");

    public static string FilePath => Path.Combine(Folder, "save.json");

    public static SaveData Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return new SaveData();
            return JsonSerializer.Deserialize<SaveData>(File.ReadAllText(FilePath), Opts) ?? new SaveData();
        }
        catch
        {
            return new SaveData();
        }
    }

    public static void Save(SaveData data)
    {
        try
        {
            Directory.CreateDirectory(Folder);
            var tmp = FilePath + ".tmp";
            File.WriteAllText(tmp, JsonSerializer.Serialize(data, Opts));
            File.Move(tmp, FilePath, overwrite: true);
        }
        catch
        {
            // Saving is best-effort; never crash the line over it.
        }
    }

    public static ContractDto Capture(KitchenState s) => new()
    {
        Week = s.Week,
        Day = s.Day,
        Budget = s.Budget,
        Hygiene = s.Hygiene,
        Morale = s.Morale,
        Favor = s.Favor,
        Headcount = s.Headcount,
        ContractStars = s.ContractStars,
        DaysPlayed = s.DaysPlayed,
        MenuA = s.MenuA?.Id ?? "",
        MenuB = s.MenuB?.Id ?? "",
        Pantry = new Dictionary<string, int>(s.Pantry),
        Owned = s.Owned.ToList(),
        Crew = s.Crew.Select(c => new TrusteeDto { Name = c.Name, Note = c.Note, Best = c.Best, Skill = c.Skill, Mood = c.Mood }).ToList(),
    };

    public static void Restore(KitchenState s, ContractDto d)
    {
        s.Mode = RunMode.Contract;
        s.Week = Math.Clamp(d.Week, 1, 8);
        s.Day = Math.Clamp(d.Day, 1, 7);
        s.Budget = d.Budget;
        s.Hygiene = Math.Clamp(d.Hygiene, 0, 100);
        s.Morale = Math.Clamp(d.Morale, 0, 100);
        s.Favor = Math.Clamp(d.Favor, 0, 100);
        s.Headcount = d.Headcount;
        s.ContractStars = d.ContractStars;
        s.DaysPlayed = d.DaysPlayed;
        s.Over = false;
        s.Ending = "";
        s.Pantry.Clear();
        foreach (var i in KitchenBook.Ingredients)
            s.Pantry[i.Id] = d.Pantry.GetValueOrDefault(i.Id);
        s.Owned.Clear();
        foreach (var id in d.Owned) s.Owned.Add(id);
        s.Crew.Clear();
        foreach (var c in d.Crew)
            s.Crew.Add(new Trustee { Name = c.Name, Note = c.Note, Best = c.Best, Skill = c.Skill, Mood = c.Mood });
        if (s.Crew.Count == 0) s.Crew.AddRange(KitchenBook.StarterCrew());
        s.MenuA = KitchenBook.Recipes.FirstOrDefault(r => r.Id == d.MenuA) ?? KitchenBook.Recipes[0];
        s.MenuB = KitchenBook.Recipes.FirstOrDefault(r => r.Id == d.MenuB && r != s.MenuA)
                  ?? KitchenBook.Recipes.First(r => r != s.MenuA);
        s.Ledger.Clear();
        s.Ledger.Add("Contract resumed.");
    }
}
