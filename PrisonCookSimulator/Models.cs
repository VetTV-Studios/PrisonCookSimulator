namespace PrisonCookSimulator;

public enum Station { Prep, Range, Fryer, Steam, Window, Pit }
public enum RunMode { Contract, Rush }

public sealed class Recipe
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Tag { get; init; }
    public required Station[] Steps { get; init; }
    public required Dictionary<string, int> Needs { get; init; }
    public int CostPerHead { get; init; }
    public int Prestige { get; init; }
    public bool Special { get; init; }
}

public sealed class Ingredient
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public int UnitCost { get; init; }
}

public sealed class Trustee
{
    public required string Name { get; set; }
    public required string Note { get; set; }
    public Station Best { get; set; }
    public int Skill { get; set; }
    public int Mood { get; set; } = 70;
    public bool Present { get; set; } = true;
}

public sealed class Upgrade
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Blurb { get; init; }
    public int Cost { get; init; }
}

public sealed class Ticket
{
    public required Recipe Recipe { get; set; }
    public int Step { get; set; }
    public double TimeLeft { get; set; }
    public int Quality { get; set; } = 78;
    public bool Ruined { get; set; }
}

public sealed class KitchenState
{
    public RunMode Mode { get; set; }
    public int Week { get; set; } = 1;
    public int Day { get; set; } = 1;
    public int Budget { get; set; } = 420;
    public int Hygiene { get; set; } = 78;
    public int Morale { get; set; } = 55;
    public int Favor { get; set; } = 50;
    public int Headcount { get; set; } = 240;
    public int MealsNeeded { get; set; } = 12;
    public int Plated { get; set; }
    public int Ruined { get; set; }
    public int ShiftScore { get; set; }
    public int BestRush { get; set; }
    public int ContractStars { get; set; }
    public bool InspectionToday { get; set; }
    public bool Lockdown { get; set; }
    public bool WardenTasting { get; set; }
    public bool Over { get; set; }
    public string Ending { get; set; } = "";
    public string Log { get; set; } = "Ironwood C.F. kitchen. You run the line.";
    public Recipe? MenuA { get; set; }
    public Recipe? MenuB { get; set; }
    public List<Trustee> Crew { get; } = new();
    public Dictionary<string, int> Pantry { get; } = new();
    public HashSet<string> Owned { get; } = new();
    public List<string> Ledger { get; } = new();

    public bool Has(string id) => Owned.Contains(id);

    public int IngredientCostMult => Has("vendor") ? 80 : 100;
}

public static class KitchenBook
{
    public static readonly Ingredient[] Ingredients =
    {
        new() { Id = "beef", Name = "Ground beef", UnitCost = 14 },
        new() { Id = "chicken", Name = "Chicken pieces", UnitCost = 12 },
        new() { Id = "beans", Name = "Pinto beans", UnitCost = 6 },
        new() { Id = "pasta", Name = "Elbow macaroni", UnitCost = 5 },
        new() { Id = "bread", Name = "Commissary bread", UnitCost = 4 },
        new() { Id = "veg", Name = "Steamed veg mix", UnitCost = 7 },
        new() { Id = "potato", Name = "Russets", UnitCost = 5 },
        new() { Id = "sauce", Name = "Red sauce drums", UnitCost = 8 },
        new() { Id = "spice", Name = "Spice crate", UnitCost = 3 },
        new() { Id = "egg", Name = "Eggs", UnitCost = 6 },
        new() { Id = "fish", Name = "Friday fish", UnitCost = 11 },
        new() { Id = "prime", Name = "Warden cut", UnitCost = 28 },
    };

    public static readonly Recipe[] Recipes =
    {
        new() { Id = "joe", Name = "Sloppy joes", Tag = "Crowd staple", CostPerHead = 3, Prestige = 1,
            Steps = new[] { Station.Prep, Station.Range, Station.Steam, Station.Window },
            Needs = new() { ["beef"] = 2, ["bread"] = 2, ["sauce"] = 1, ["spice"] = 1 } },
        new() { Id = "mac", Name = "Chili mac", Tag = "Fills the tray", CostPerHead = 3, Prestige = 1,
            Steps = new[] { Station.Prep, Station.Range, Station.Steam, Station.Window },
            Needs = new() { ["beef"] = 1, ["pasta"] = 2, ["beans"] = 1, ["sauce"] = 1 } },
        new() { Id = "loaf", Name = "Mystery loaf", Tag = "Don't ask", CostPerHead = 2, Prestige = 0,
            Steps = new[] { Station.Prep, Station.Range, Station.Window },
            Needs = new() { ["beef"] = 1, ["bread"] = 1, ["egg"] = 1, ["spice"] = 1 } },
        new() { Id = "beans", Name = "Beans & skillet bread", Tag = "Budget day", CostPerHead = 2, Prestige = 0,
            Steps = new[] { Station.Prep, Station.Range, Station.Steam, Station.Window },
            Needs = new() { ["beans"] = 3, ["bread"] = 1, ["spice"] = 1 } },
        new() { Id = "king", Name = "Chicken a la yard", Tag = "Looks official", CostPerHead = 4, Prestige = 2,
            Steps = new[] { Station.Prep, Station.Range, Station.Steam, Station.Window },
            Needs = new() { ["chicken"] = 2, ["sauce"] = 1, ["veg"] = 1, ["spice"] = 1 } },
        new() { Id = "hash", Name = "Breakfast hash", Tag = "Dawn count", CostPerHead = 3, Prestige = 1,
            Steps = new[] { Station.Prep, Station.Range, Station.Fryer, Station.Window },
            Needs = new() { ["potato"] = 2, ["egg"] = 2, ["spice"] = 1 } },
        new() { Id = "fish", Name = "Friday fish & chips", Tag = "Chapel day", CostPerHead = 4, Prestige = 2,
            Steps = new[] { Station.Prep, Station.Fryer, Station.Steam, Station.Window },
            Needs = new() { ["fish"] = 2, ["potato"] = 2, ["spice"] = 1 } },
        new() { Id = "veg", Name = "Garden loaf", Tag = "Dietary line", CostPerHead = 3, Prestige = 2,
            Steps = new[] { Station.Prep, Station.Range, Station.Steam, Station.Window },
            Needs = new() { ["veg"] = 2, ["beans"] = 1, ["bread"] = 1, ["spice"] = 1 } },
        new() { Id = "stew", Name = "Ironwood stew", Tag = "Cold snap", CostPerHead = 3, Prestige = 1,
            Steps = new[] { Station.Prep, Station.Range, Station.Steam, Station.Window },
            Needs = new() { ["beef"] = 1, ["potato"] = 2, ["veg"] = 1, ["spice"] = 1 } },
        new() { Id = "warden", Name = "Warden's plate", Tag = "Do not burn this", CostPerHead = 8, Prestige = 4, Special = true,
            Steps = new[] { Station.Prep, Station.Range, Station.Fryer, Station.Steam, Station.Window },
            Needs = new() { ["prime"] = 1, ["potato"] = 1, ["veg"] = 1, ["spice"] = 1 } },
    };

    public static readonly Upgrade[] Shop =
    {
        new() { Id = "knives", Name = "Knife roll", Blurb = "Prep hits are more forgiving.", Cost = 80 },
        new() { Id = "oven", Name = "Convection oven", Blurb = "Range steps last longer.", Cost = 220 },
        new() { Id = "fry", Name = "New fryer baskets", Blurb = "Fryer quality floor rises.", Cost = 140 },
        new() { Id = "vendor", Name = "Better vendor", Blurb = "Ingredients cost 20% less.", Cost = 120 },
        new() { Id = "drains", Name = "Floor drains", Blurb = "Hygiene drops slower on the line.", Cost = 90 },
        new() { Id = "window", Name = "Pass window rails", Blurb = "Window steps are quicker.", Cost = 160 },
        new() { Id = "crew", Name = "Fourth trustee", Blurb = "Hire one more pair of hands.", Cost = 150 },
    };

    public static readonly string[] Days =
    {
        "Monday — standard count",
        "Tuesday — dietary trays",
        "Wednesday — inspector rumor",
        "Thursday — movement freeze risk",
        "Friday — fish & chapel",
        "Saturday — leftover math",
        "Sunday — tasting or inventory",
    };

    public static Trustee[] StarterCrew() =>
    [
        new() { Name = "Delroy Pike", Note = "Range lifer", Best = Station.Range, Skill = 78 },
        new() { Name = "Mara Quinn", Note = "Fast prep", Best = Station.Prep, Skill = 74 },
        new() { Name = "Theo Briggs", Note = "Window talker", Best = Station.Window, Skill = 70 },
    ];

    public static Trustee RandomHire(Random rng)
    {
        string[] names = ["Sami Ortiz", "Hank Voss", "June Pell", "Rico Hale", "Ned Graves", "Ivy Cho"];
        string[] notes = ["Quiet knife", "Steam table saint", "Fryer gambler", "Pit specialist", "Reads the tickets"];
        var stations = Enum.GetValues<Station>();
        return new Trustee
        {
            Name = names[rng.Next(names.Length)],
            Note = notes[rng.Next(notes.Length)],
            Best = stations[rng.Next(stations.Length)],
            Skill = rng.Next(62, 86),
            Mood = rng.Next(55, 80),
        };
    }

    public static void StockStarter(KitchenState s)
    {
        s.Pantry.Clear();
        foreach (var i in Ingredients)
            s.Pantry[i.Id] = i.Id is "prime" or "fish" ? 1 : 4;
        s.Crew.Clear();
        s.Crew.AddRange(StarterCrew());
        s.MenuA = Recipes[0];
        s.MenuB = Recipes[1];
        s.Ledger.Clear();
        s.Ledger.Add("Contract signed. Allotment posted.");
    }

    public static Ingredient? FindItem(string id) => Ingredients.FirstOrDefault(i => i.Id == id);

    public static int BuyCost(KitchenState s, Ingredient item, int qty)
        => (int)Math.Ceiling(item.UnitCost * qty * s.IngredientCostMult / 100.0);

    public static bool CanCook(KitchenState s, Recipe r)
    {
        foreach (var kv in r.Needs)
            if (s.Pantry.GetValueOrDefault(kv.Key) < kv.Value) return false;
        return true;
    }

    public static void SpendRecipe(KitchenState s, Recipe r)
    {
        foreach (var kv in r.Needs)
            s.Pantry[kv.Key] = Math.Max(0, s.Pantry.GetValueOrDefault(kv.Key) - kv.Value);
    }

    public static string DayName(int day) => Days[Math.Clamp(day - 1, 0, 6)];
}
