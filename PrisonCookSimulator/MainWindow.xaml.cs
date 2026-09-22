using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace PrisonCookSimulator;

public sealed partial class MainWindow : Window
{
    private readonly KitchenState _s = new();
    private readonly Random _rng = new();
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(50) };
    private readonly List<Ticket> _queue = new();
    private Ticket? _live;
    private int _recipeIndexA;
    private int _recipeIndexB = 1;
    private bool _lineLive;
    private double _hygieneAcc;

    public MainWindow()
    {
        InitializeComponent();
        Title = "Prison Cook Simulator";
        ExtendsContentIntoTitleBar = true;
        _timer.Tick += OnTick;
        BuyBox.ItemsSource = KitchenBook.Ingredients.Select(i => i.Name).ToList();
        if (BuyBox.Items.Count > 0) BuyBox.SelectedIndex = 0;
        ShopList.ItemsSource = KitchenBook.Shop.Select(u => $"{u.Name}  ·  ${u.Cost}").ToList();
    }

    private void Show(Grid panel)
    {
        MenuPanel.Visibility = Visibility.Collapsed;
        HelpPanel.Visibility = Visibility.Collapsed;
        HubPanel.Visibility = Visibility.Collapsed;
        LinePanel.Visibility = Visibility.Collapsed;
        ReportPanel.Visibility = Visibility.Collapsed;
        EndPanel.Visibility = Visibility.Collapsed;
        panel.Visibility = Visibility.Visible;
    }

    private void OnMenu(object sender, RoutedEventArgs e)
    {
        _timer.Stop();
        _lineLive = false;
        Show(MenuPanel);
    }

    private void OnHelp(object sender, RoutedEventArgs e) => Show(HelpPanel);

    private void OnContract(object sender, RoutedEventArgs e)
    {
        ResetCampaign(RunMode.Contract);
        PaintHub();
        Show(HubPanel);
    }

    private void OnRush(object sender, RoutedEventArgs e)
    {
        ResetCampaign(RunMode.Rush);
        StartLine();
    }

    private void ResetCampaign(RunMode mode)
    {
        _s.Mode = mode;
        _s.Week = 1;
        _s.Day = 1;
        _s.Budget = 420;
        _s.Hygiene = 78;
        _s.Morale = 55;
        _s.Favor = 50;
        _s.Headcount = 240;
        _s.Plated = 0;
        _s.Ruined = 0;
        _s.ShiftScore = 0;
        _s.ContractStars = 0;
        _s.Over = false;
        _s.Ending = "";
        _s.Owned.Clear();
        KitchenBook.StockStarter(_s);
        _recipeIndexA = 0;
        _recipeIndexB = 1;
        _s.MenuA = KitchenBook.Recipes[_recipeIndexA];
        _s.MenuB = KitchenBook.Recipes[_recipeIndexB];
        RollDayFlags();
        _s.Log = mode == RunMode.Contract
            ? "Week 1. Allotment is posted. The count eats what you plate."
            : "Rush line. No budget. Do not ruin three trays.";
    }

    private void RollDayFlags()
    {
        _s.InspectionToday = _s.Day == 3 || _rng.NextDouble() < 0.12;
        _s.Lockdown = _s.Day == 4 && _rng.NextDouble() < 0.45;
        _s.WardenTasting = _s.Day == 7 || (_s.Day == 5 && _rng.NextDouble() < 0.2);
        foreach (var c in _s.Crew) c.Present = true;
        if (_s.Lockdown && _s.Crew.Count > 0)
        {
            var pulled = _s.Crew[_rng.Next(_s.Crew.Count)];
            pulled.Present = false;
            _s.Log = $"Movement freeze. {pulled.Name} is held on the block.";
        }
        else if (_s.InspectionToday)
            _s.Log = "Rumor from the gate: inspector badge in the lot.";
        else if (_s.WardenTasting)
            _s.Log = "Warden put himself on the tasting list. Do not send loaf.";
        else
            _s.Log = KitchenBook.DayName(_s.Day) + ".";
    }

    private void PaintHub()
    {
        HubTitle.Text = _s.Mode == RunMode.Contract
            ? $"Week {_s.Week}  ·  {KitchenBook.DayName(_s.Day)}"
            : "Rush line";
        HubMeta.Text = _s.InspectionToday ? "INSPECTION FLAG" : _s.Lockdown ? "LOCKDOWN STAFFING" : _s.WardenTasting ? "WARDEN TASTING" : "Normal movement";
        HubLog.Text = _s.Log;
        StatBudget.Text = $"${_s.Budget}";
        StatHyg.Text = $"Hygiene {_s.Hygiene}";
        StatMorale.Text = $"Morale {_s.Morale}";
        StatFavor.Text = $"Warden {_s.Favor}";
        MenuABtn.Content = _s.MenuA is null ? "Menu A" : _s.MenuA.Name;
        MenuBBtn.Content = _s.MenuB is null ? "Menu B" : _s.MenuB.Name;
        CrewList.ItemsSource = _s.Crew.Select(c =>
            $"{c.Name}  ·  {c.Best} {c.Skill}  ·  {(c.Present ? c.Note : "PULLED")}").ToList();
        PantryList.ItemsSource = KitchenBook.Ingredients
            .Select(i => $"{i.Name,-22}  {_s.Pantry.GetValueOrDefault(i.Id),2}   ${KitchenBook.BuyCost(_s, i, 1)}/ea")
            .ToList();
        ShopList.ItemsSource = KitchenBook.Shop.Select(u =>
            _s.Has(u.Id) ? $"{u.Name}  ·  owned" : $"{u.Name}  ·  ${u.Cost}").ToList();
    }

    private void OnCycleMenuA(object sender, RoutedEventArgs e)
    {
        _recipeIndexA = (_recipeIndexA + 1) % KitchenBook.Recipes.Length;
        if (_recipeIndexA == _recipeIndexB) _recipeIndexA = (_recipeIndexA + 1) % KitchenBook.Recipes.Length;
        _s.MenuA = KitchenBook.Recipes[_recipeIndexA];
        PaintHub();
    }

    private void OnCycleMenuB(object sender, RoutedEventArgs e)
    {
        _recipeIndexB = (_recipeIndexB + 1) % KitchenBook.Recipes.Length;
        if (_recipeIndexB == _recipeIndexA) _recipeIndexB = (_recipeIndexB + 1) % KitchenBook.Recipes.Length;
        _s.MenuB = KitchenBook.Recipes[_recipeIndexB];
        PaintHub();
    }

    private void OnBuyCrate(object sender, RoutedEventArgs e)
    {
        var idx = BuyBox.SelectedIndex;
        if (idx < 0) return;
        var item = KitchenBook.Ingredients[idx];
        var cost = KitchenBook.BuyCost(_s, item, 1);
        if (_s.Budget < cost)
        {
            _s.Log = "Vendor wants cash up front.";
            PaintHub();
            return;
        }
        _s.Budget -= cost;
        _s.Pantry[item.Id] = _s.Pantry.GetValueOrDefault(item.Id) + 1;
        _s.Ledger.Add($"Bought {item.Name} (−${cost})");
        _s.Log = $"Crate in: {item.Name}.";
        PaintHub();
    }

    private void OnBuyUpgrade(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not string label) return;
        var up = KitchenBook.Shop.FirstOrDefault(u => label.StartsWith(u.Name, StringComparison.Ordinal));
        if (up is null || _s.Has(up.Id)) return;
        if (_s.Budget < up.Cost)
        {
            ShopHint.Text = "Not enough allotment.";
            return;
        }
        _s.Budget -= up.Cost;
        _s.Owned.Add(up.Id);
        if (up.Id == "crew")
            _s.Crew.Add(KitchenBook.RandomHire(_rng));
        _s.Log = $"Installed {up.Name}.";
        ShopHint.Text = up.Blurb;
        PaintHub();
    }

    private void OnScrub(object sender, RoutedEventArgs e)
    {
        if (_s.Budget < 25)
        {
            _s.Log = "No soap money.";
            PaintHub();
            return;
        }
        _s.Budget -= 25;
        _s.Hygiene = Math.Min(100, _s.Hygiene + 14);
        _s.Log = "Floor shining. Inspector would still find a corner.";
        PaintHub();
    }

    private void OnOpenLine(object sender, RoutedEventArgs e)
    {
        if (_s.MenuA is null || _s.MenuB is null) return;
        if (!KitchenBook.CanCook(_s, _s.MenuA) && !KitchenBook.CanCook(_s, _s.MenuB))
        {
            _s.Log = "Pantry is empty. Buy crates or the count eats air.";
            PaintHub();
            return;
        }
        StartLine();
    }

    private void StartLine()
    {
        _s.Plated = 0;
        _s.Ruined = 0;
        _s.ShiftScore = 0;
        _s.MealsNeeded = _s.Mode == RunMode.Rush ? 20 : (_s.WardenTasting ? 10 : 12);
        if (_s.Lockdown) _s.MealsNeeded = Math.Max(8, _s.MealsNeeded - 2);
        _queue.Clear();
        _live = null;
        _hygieneAcc = 0;
        ServiceBar.Maximum = _s.MealsNeeded;
        ServiceBar.Value = 0;
        SeedTickets(3);
        PullTicket();
        _lineLive = true;
        _timer.Start();
        LineToast.Text = _s.InspectionToday ? "Hairnet check. Hygiene is on the clock." : "First ticket up.";
        PaintLine();
        Show(LinePanel);
    }

    private Recipe PickRecipe()
    {
        var options = new List<Recipe>();
        if (_s.Mode == RunMode.Rush)
        {
            options.AddRange(KitchenBook.Recipes.Where(r => !r.Special || _rng.NextDouble() < 0.08));
        }
        else
        {
            if (_s.WardenTasting) options.Add(KitchenBook.Recipes.First(r => r.Id == "warden"));
            if (_s.MenuA is not null && KitchenBook.CanCook(_s, _s.MenuA)) options.Add(_s.MenuA);
            if (_s.MenuB is not null && KitchenBook.CanCook(_s, _s.MenuB)) options.Add(_s.MenuB);
            if (options.Count == 0)
            {
                var fallback = KitchenBook.Recipes.FirstOrDefault(r => KitchenBook.CanCook(_s, r));
                options.Add(fallback ?? KitchenBook.Recipes[3]);
            }
        }
        return options[_rng.Next(options.Count)];
    }

    private void SeedTickets(int n)
    {
        for (var i = 0; i < n; i++) EnqueueOne();
    }

    private void EnqueueOne()
    {
        var r = PickRecipe();
        if (_s.Mode == RunMode.Contract && KitchenBook.CanCook(_s, r))
            KitchenBook.SpendRecipe(_s, r);
        var window = StepWindow(r);
        _queue.Add(new Ticket { Recipe = r, TimeLeft = window, Quality = 80 });
    }

    private double StepWindow(Recipe r)
    {
        var t = 4.2 + r.Steps.Length * 0.15;
        if (_s.Has("oven")) t += 0.45;
        if (_s.Has("window")) t += 0.15;
        if (_s.Lockdown) t -= 0.35;
        return t;
    }

    private void PullTicket()
    {
        if (_queue.Count == 0) EnqueueOne();
        _live = _queue[0];
        _queue.RemoveAt(0);
        _live.Step = 0;
        _live.TimeLeft = StepWindow(_live.Recipe);
        if (_queue.Count < 3 && _s.Plated + _s.Ruined + 1 + _queue.Count < _s.MealsNeeded + 2)
            EnqueueOne();
    }

    private void OnTick(object sender, object e)
    {
        if (!_lineLive || _live is null) return;
        _live.TimeLeft -= 0.05;
        _hygieneAcc += _s.Has("drains") ? 0.012 : 0.02;
        if (_hygieneAcc >= 1)
        {
            _hygieneAcc = 0;
            _s.Hygiene = Math.Max(0, _s.Hygiene - 1);
        }
        if (_live.TimeLeft <= 0)
        {
            Ruin("Ticket died on the pass.");
            return;
        }
        PaintLine();
    }

    private void OnStation(object sender, RoutedEventArgs e)
    {
        if (!_lineLive || _live is null) return;
        if (sender is not Button btn || btn.Tag is not string tag) return;
        if (!Enum.TryParse<Station>(tag, out var st)) return;
        var need = _live.Recipe.Steps[Math.Clamp(_live.Step, 0, _live.Recipe.Steps.Length - 1)];
        if (st != need)
        {
            _live.Quality -= 16;
            LineToast.Text = $"Wrong station. Needed {need}.";
            if (_live.Quality <= 30) Ruin("Tray dumped. Quality gone.");
            else PaintLine();
            return;
        }

        var ratio = _live.TimeLeft / Math.Max(0.2, StepWindow(_live.Recipe));
        var hit = ratio > 0.55 ? 8 : ratio > 0.25 ? 3 : -6;
        if (CrewBonus(st)) hit += 6;
        if (st == Station.Prep && _s.Has("knives")) hit += 3;
        if (st == Station.Fryer && _s.Has("fry")) hit += 3;
        _live.Quality = Math.Clamp(_live.Quality + hit, 5, 100);
        _live.Step++;
        if (_live.Step >= _live.Recipe.Steps.Length)
        {
            Plate(_live);
            return;
        }
        _live.TimeLeft = StepWindow(_live.Recipe);
        LineToast.Text = hit >= 8 ? "Clean hit." : hit >= 3 ? "Good enough." : "Late. Still edible.";
        PaintLine();
    }

    private bool CrewBonus(Station st)
        => _s.Crew.Any(c => c.Present && c.Best == st && c.Skill >= 70);

    private void Plate(Ticket t)
    {
        _s.Plated++;
        var q = t.Quality;
        _s.ShiftScore += q;
        ServiceBar.Value = _s.Plated;
        LineToast.Text = q >= 85 ? $"{t.Recipe.Name} — excellent." : q >= 65 ? $"{t.Recipe.Name} — out the window." : $"{t.Recipe.Name} — they will eat it.";
        if (_s.Plated >= _s.MealsNeeded)
        {
            EndShift(false);
            return;
        }
        PullTicket();
        PaintLine();
    }

    private void Ruin(string why)
    {
        _s.Ruined++;
        if (_live is not null) LineToast.Text = why + $"  ({_live.Recipe.Name})";
        if (_s.Ruined >= 3)
        {
            EndShift(true);
            return;
        }
        PullTicket();
        PaintLine();
    }

    private void OnPit(object sender, RoutedEventArgs e)
    {
        if (!_lineLive) return;
        _s.Hygiene = Math.Min(100, _s.Hygiene + 8);
        if (_live is not null) _live.TimeLeft = Math.Max(0.4, _live.TimeLeft - 0.9);
        LineToast.Text = "Pit run. Hygiene up, ticket clock down.";
        PaintLine();
    }

    private void OnAbandon(object sender, RoutedEventArgs e)
    {
        if (_s.Mode == RunMode.Rush)
        {
            _timer.Stop();
            _lineLive = false;
            ShowRushEnd(bailed: true);
            return;
        }
        _s.Morale = Math.Max(0, _s.Morale - 12);
        _s.Favor = Math.Max(0, _s.Favor - 8);
        EndShift(true);
    }

    private void PaintLine()
    {
        LineHud.Text = _s.Mode == RunMode.Rush
            ? $"RUSH  ·  best {_s.BestRush}"
            : $"WEEK {_s.Week}  DAY {_s.Day}  ·  hyg {_s.Hygiene}";
        LineStats.Text = $"Plated {_s.Plated}/{_s.MealsNeeded}   Ruined {_s.Ruined}/3   Score {_s.ShiftScore}";
        QueueText.Text = string.Join("\n", _queue.Take(6).Select((t, i) => $"{i + 1}. {t.Recipe.Name}"));
        if (_live is null)
        {
            TicketName.Text = "No ticket";
            TicketTag.Text = "";
            TicketSteps.Text = "";
            TicketBar.Value = 0;
            TicketHint.Text = "";
            return;
        }
        TicketName.Text = _live.Recipe.Name;
        TicketTag.Text = $"{_live.Recipe.Tag}  ·  quality {_live.Quality}";
        var marks = _live.Recipe.Steps.Select((st, i) => i < _live.Step ? $"[{st}]" : i == _live.Step ? $"» {st} «" : st.ToString());
        TicketSteps.Text = string.Join("   →   ", marks);
        var max = StepWindow(_live.Recipe);
        TicketBar.Value = Math.Clamp(_live.TimeLeft / max, 0, 1);
        TicketHint.Text = CrewBonus(_live.Recipe.Steps[Math.Min(_live.Step, _live.Recipe.Steps.Length - 1)])
            ? "Trustee on this station."
            : "No specialist on this hit.";
    }

    private void EndShift(bool collapse)
    {
        _timer.Stop();
        _lineLive = false;
        if (_s.Mode == RunMode.Rush)
        {
            ShowRushEnd(bailed: collapse && _s.Plated == 0);
            return;
        }

        var avg = _s.Plated == 0 ? 0 : _s.ShiftScore / Math.Max(1, _s.Plated);
        _s.Morale = Math.Clamp(_s.Morale + (avg >= 80 ? 8 : avg >= 60 ? 3 : -10) - _s.Ruined * 4, 0, 100);
        _s.Favor = Math.Clamp(_s.Favor + (_s.WardenTasting ? (avg >= 82 ? 12 : -10) : avg >= 75 ? 3 : -2), 0, 100);
        if (_s.InspectionToday)
        {
            if (_s.Hygiene < 45)
            {
                FailContract("Inspector shut the line. Hygiene 45 was the floor. You missed it.");
                return;
            }
            _s.Favor = Math.Min(100, _s.Favor + 4);
        }
        if (_s.Morale <= 0)
        {
            FailContract("Trays came back full. The block refused the window. Contract pulled.");
            return;
        }

        var pay = 70 + _s.Plated * 4 + (_s.WardenTasting && avg >= 82 ? 40 : 0);
        _s.Budget += pay;
        if (_s.Day == 1) _s.Budget += 180;
        _s.ContractStars += avg >= 85 ? 3 : avg >= 70 ? 2 : 1;

        ReportBody.Text =
            $"{KitchenBook.DayName(_s.Day)}\n\n" +
            $"Plated {_s.Plated}  ·  ruined {_s.Ruined}  ·  avg quality {avg}\n" +
            $"Allotment +${pay}. Cash on hand ${_s.Budget}.\n" +
            $"Morale {_s.Morale}  ·  Warden {_s.Favor}  ·  Hygiene {_s.Hygiene}\n\n" +
            (collapse ? "You left tickets on the board. The count noticed." : "Window closed. Next count is already posted.");
        _s.Log = collapse ? "Short shift. Notes in the warden's book." : $"Avg {avg}. Paid ${pay}.";
        Show(ReportPanel);
    }

    private void ShowRushEnd(bool bailed)
    {
        if (_s.ShiftScore > _s.BestRush) _s.BestRush = _s.ShiftScore;
        EndTitle.Text = bailed ? "Line abandoned" : "Rush over";
        EndBody.Text = $"Plated {_s.Plated}. Ruined {_s.Ruined}. Score {_s.ShiftScore}. Best {_s.BestRush}.";
        Show(EndPanel);
    }

    private void OnReportDone(object sender, RoutedEventArgs e)
    {
        if (_s.Over)
        {
            Show(EndPanel);
            return;
        }
        AdvanceDay();
        if (_s.Over)
        {
            Show(EndPanel);
            return;
        }
        PaintHub();
        Show(HubPanel);
    }

    private void AdvanceDay()
    {
        _s.Day++;
        if (_s.Day > 7)
        {
            _s.Day = 1;
            _s.Week++;
            foreach (var c in _s.Crew)
                c.Mood = Math.Clamp(c.Mood + 4, 40, 95);
        }
        if (_s.Week > 8)
        {
            FinishContract();
            return;
        }
        RollDayFlags();
        if (_s.Hygiene < 25) _s.Log += " Floors are a health write-up waiting.";
    }

    private void FinishContract()
    {
        _s.Over = true;
        var stars = Math.Clamp(_s.ContractStars / 8, 1, 3);
        EndTitle.Text = stars >= 3 ? "Contract renewed" : stars == 2 ? "Contract closed" : "They will not call back";
        EndBody.Text =
            $"Eight weeks at Ironwood.\nMorale {_s.Morale}. Warden {_s.Favor}. Hygiene {_s.Hygiene}. Cash ${_s.Budget}.\n" +
            $"Service stars: {stars} / 3.\n\n" +
            (stars >= 3
                ? "The inspector signed the book. The warden wants you for the next fiscal year."
                : stars == 2
                    ? "Nobody starved. That is the official compliment."
                    : "You fed the count. Barely. Pack the knives.");
    }

    private void FailContract(string why)
    {
        _s.Over = true;
        _s.Ending = why;
        EndTitle.Text = "Contract pulled";
        EndBody.Text = why;
        Show(EndPanel);
    }
}
