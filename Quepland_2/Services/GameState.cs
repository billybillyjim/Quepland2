using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Pluralize.NET;
using Quepland_2.Components;
using Quepland_2.Pages;
using Quepland_2.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
//using Pluralize.NET;


    public class GameState
    {
    public enum GameType
    {
        Normal,
        Hardcore,
        Ultimate
    }
    public static GameType CurrentGameMode;
    public event EventHandler StateChanged;
    public IJSRuntime JSRuntime;

    public static string Version { get; set; } = "1.0.15";
    public static List<Update> Updates { get; set; } = new List<Update>();
    public static Pluralizer Pluralizer = new Pluralizer();

    public static string Location { get; set; } = "";
    public static string BGColor { get; set; } = "#2d2d2d";
    public static bool InitCompleted { get; set; } = false;
    public static bool ShowStartMenu { get; set; } = true;
    public static bool ShowSettings { get; set; }
    public static bool ShowExpTrackerSettings { get; set; }
    private bool stopActions = false;
    private bool stopNoncombatActions = false;
    public bool IsStoppingNextTick { get; set; }
    private static bool cancelTask;
    public static bool IsOnHuntingTrip { get; set; }
    public static bool UseNewSaveCompression { get; set; }
    public static AFKAction CurrentAFKAction { get; set; }
    private Timer GameTimer { get; set; }
    public int testInt = 0;
    private static Guid _guid;
    public static Guid Guid
    {
        get
        {
            if (_guid == Guid.Empty)
            {
                _guid = Guid.NewGuid();
                return _guid;
            }
            else
            {
                return _guid;
            }
        }
        set
        {
            _guid = value;
        }
    }

    public GameItem NewGatherItem;
    public GameItem NewRequiredItem;
    public Recipe NewCraftingRecipe;
    public Recipe NewSmeltingRecipe;
    public Recipe NewSmithingRecipe;
    public Book NewBook;

    public GameItem CurrentGatherItem;
    public GameItem RequiredForGatherItem;
    public List<GameItem> PossibleGatherItems = new List<GameItem>();
    public Recipe CurrentSmeltingRecipe;
    public Recipe CurrentSmithingRecipe;
    public Book CurrentBook;

    public AlchemicalFormula CurrentAlchemyFormula;
    
    public GameItem CurrentFood;
    public static bool CancelEating;
    public Recipe CurrentRecipe;
    public static Land CurrentLand;
    public ItemViewerComponent itemViewer;
    public InventoryViewerComponent inventoryViewer;
    public SmithyComponent SmithingComponent;
    public OvenComponent OvenComponent;
    public CraftingComponent CraftingComponent;
    public AlchemicalHallComponent AlchemicalHallComponent;
    public NavMenu NavMenu;
    public ContextMenu CurrentContextMenu;
    public RightSidebarComponent RightSidebarComponent;
    public ExperienceTrackerComponent EXPTrackerComponent;
    public static NavigationManager UriHelper;
    public static ArtisanTask CurrentArtisanTask;
    public static int TicksToNextAction;
    public static readonly int GameSpeed = 200;
    public static bool CompactInventoryView;
    public static bool HideLockedItems;
    public int TicksToNextHeal;
    public int HealingTicks;
    public static int CurrentTick { get; set; }
    public static int AutoSaveInterval { get; set; } = 4500;
    public static int GameWindowWidth { get; set; }
    public static int GameWindowHeight { get; set; }
    public static int GameLoadProgress { get; set; }
    public static bool ShowTome { get; set; }
    public static string CurrentTome { get; set; } = "None";
    public int MinWindowWidth { get; set; } = 600;
    public int AlchemyStage;
    public int AutoSmithedItemCount { get; set; } = 0;

    private QuestTester QuestTester = new QuestTester();
    private RecipeTester RecipeTester = new RecipeTester();
    public static Random Random = new Random();

    public bool SaveGame = false;
    public static bool IsSaving = false;
    private Stopwatch stopwatch = new Stopwatch();

    public void Start()
    {
        if(GameTimer != null)
        {
            return;
        }
        //QuestTester.TestQuests();
        //RecipeTester.TestRecipes();
        GameTimer = new Timer(new TimerCallback(async _ =>
        {
            await Tick();
            StateHasChanged();
        }), null, GameSpeed, GameSpeed);
        StateHasChanged();
        //QuestTester.TestQuests();
        //RecipeTester.TestRecipes();
    }

    private async Task Tick()
    {
        if (IsOnHuntingTrip || CurrentAFKAction != null)
        {
            if (SaveGame)
            {
                IsSaving = true;
                await SaveManager.SaveGame();
                SaveGame = false;
            }
            else if (CurrentTick % AutoSaveInterval == 0)
            {
                await SaveManager.SaveGame();
            }
            CurrentTick++;
            StateHasChanged();
            return;
        }
        if (stopActions)
        {
            ClearActions();
        }
        else if (stopNoncombatActions)
        {
            ClearNonCombatActions(); 
        }
        if (TicksToNextAction <= 0 && CurrentGatherItem != null)
        {
            GatherItem();
        }
        else if (TicksToNextAction <= 0 && CurrentRecipe != null)
        {
            CraftItem();
        }
        else if (TicksToNextAction <= 0 && CurrentSmithingRecipe != null && CurrentSmeltingRecipe != null)
        {
            SmithItem();
        }
        else if (TicksToNextAction <= 0 && CurrentAlchemyFormula != null)
        {
            AlchItem();
        }
        else if (BattleManager.Instance.CurrentOpponents != null && BattleManager.Instance.CurrentOpponents.Count > 0)
        {
            if (BattleManager.Instance.WaitedAutoBattleGameTick)
            {
                if (BattleManager.Instance.SelectedOpponent != null)
                {
                    BattleManager.Instance.StartBattle(BattleManager.Instance.SelectedOpponent);
                }
                else
                {
                    BattleManager.Instance.StartBattle(BattleManager.Instance.CurrentArea);
                }
            }
            else
            {
                BattleManager.Instance.DoBattle();
            }

        }
        else if(TicksToNextAction <= 0 && CurrentBook != null)
        {
            ReadBook();
        }
        if (Player.Instance.CurrentFollower != null && Player.Instance.CurrentFollower.IsBanking)
        {
            Player.Instance.CurrentFollower.TicksToNextAction--;
            if (Player.Instance.CurrentFollower.TicksToNextAction <= 0)
            {
                Player.Instance.CurrentFollower.BankItems();
            }
        }
        if (CurrentFood != null && CurrentTick % CurrentFood.FoodInfo.HealSpeed == 0)
        {
            if (CancelEating)
            {
                CurrentFood = null;
                CancelEating = false;
                Player.Instance.ClearBoosts();
            }
            else
            {
                HealPlayer();
            }
        }
        
        if (EXPTrackerComponent != null)
        {
            if (CurrentTick % 5 == 0)
            {
                foreach (ExperienceTracker t in EXPTrackerComponent.TrackedExperinceRates)
                {
                    t.TimeSinceTrackerStarted += TimeSpan.FromMilliseconds(GameSpeed * 5);
                }
            }

        }
        if (Player.Instance.JustDied)
        {
            CurrentFood = null;
            HealingTicks = 0;
            Player.Instance.ClearBoosts();
            Player.Instance.JustDied = false;
        }
        if (cancelTask)
        {
            CurrentArtisanTask = null;
            cancelTask = false;
        }
        Player.Instance.TickStatusEffects();
        await GetDimensions();
        TicksToNextAction--;
        CurrentTick++;
        TooltipManager.currentDelay++;
        if (SaveGame)
        {
            IsSaving = true;
            await SaveManager.SaveGame();
            SaveGame = false;
        }
        else if (CurrentTick % AutoSaveInterval == 0)
        {
            await SaveManager.SaveGame();
        }
    }
    /// <summary>
    /// Pauses actions at the beginning of the next game tick.
    /// </summary>
    public void StopActions()
    {
        stopActions = true;
        IsStoppingNextTick = true;
    }
    public void StopNonCombatActions()
    {
        stopNoncombatActions = true;
        IsStoppingNextTick = true;
    }
    private void ClearActions()
    {
        CurrentGatherItem = null;
        RequiredForGatherItem = null;
        CurrentRecipe = null;
        BattleManager.Instance.CurrentOpponents.Clear();
        CurrentSmithingRecipe = null;
        CurrentAlchemyFormula = null;
        AlchemyStage = 0;
        SmithingManager.SmithingStage = 0;
        CurrentSmeltingRecipe = null;
        CurrentBook = null;
        stopActions = false;
        IsStoppingNextTick = false;
        if(NewGatherItem != null)
        {
            CurrentGatherItem = NewGatherItem;
            NewGatherItem = null;
        }
        if(NewRequiredItem != null)
        {
            RequiredForGatherItem = NewRequiredItem;
            NewRequiredItem = null;
        }
        if(NewSmithingRecipe != null)
        {
            CurrentSmithingRecipe = NewSmithingRecipe;
            NewSmithingRecipe = null; 

        }
        if (NewSmeltingRecipe != null)
        {
            CurrentSmeltingRecipe = NewSmeltingRecipe;
            NewSmeltingRecipe = null;
        }
        if(NewCraftingRecipe != null)
        {
            CurrentRecipe = NewCraftingRecipe;
            NewCraftingRecipe = null;
        }
        if (NewBook != null)
        {
            CurrentBook = NewBook;
            NewBook = null;
        }    
    }
    public static void CancelTask()
    {
        cancelTask = true;
    }
    private void ClearNonCombatActions()
    {
        CurrentGatherItem = null;
        CurrentRecipe = null;
        CurrentSmithingRecipe = null;
        CurrentSmeltingRecipe = null;
        stopNoncombatActions = false;
        IsStoppingNextTick = false;
    }

    public void Pause()
    {
        GameTimer.Change(Timeout.Infinite, Timeout.Infinite);
    }

    private void GatherItem()
    {
        if (HasRequiredItemForGather())
        {
            if (Player.Instance.FollowerGatherItem(CurrentGatherItem) == false)
            {
                PlayerGatherItem();
            }
            if (CurrentGatherItem != null)
            {
                if (Player.Instance.CurrentFollower != null && Player.Instance.CurrentFollower.IsBanking && Player.Instance.CurrentFollower.InventorySize > 0 && Player.Instance.Inventory.GetAvailableSpaces() == 0)
                {

                    TicksToNextAction = Player.Instance.CurrentFollower.TicksToNextAction;

                }
                else
                {
                    if (PossibleGatherItems.Count > 1)
                    {
                        int roll = Random.Next(0, PossibleGatherItems.Count);
                        CurrentGatherItem = PossibleGatherItems[roll];
                    }
                    TicksToNextAction = GetTicksToNextGather();
                }
            }
        }
    }
    private bool PlayerGatherItem()
    {

        if (Player.Instance.PlayerGatherItem(CurrentGatherItem) == false)
        {
            if(Player.Instance.CurrentFollower != null && Player.Instance.CurrentFollower.InventorySize > 0)
            {
                if (Player.Instance.CurrentFollower.IsBanking == false)
                {
                    CurrentGatherItem = null;
                    return false;
                }
            }

        }
        if(Player.Instance.Inventory.GetAvailableSpaces() == 0 && RequiredForGatherItem == null && 
            !(CurrentGatherItem != null && CurrentGatherItem.IsStackable && Player.Instance.Inventory.HasItem(CurrentGatherItem)))
        {
            if(Player.Instance.CurrentFollower != null && Player.Instance.CurrentFollower.IsBanking == false)
            {
                CurrentGatherItem = null;
            }
            else if(Player.Instance.CurrentFollower != null)
            {
                TicksToNextAction = Player.Instance.CurrentFollower.TicksToNextAction;
            }
            else
            {
                CurrentGatherItem = null;
            }
        }
        return true;
        
    }
    private bool HasRequiredItemForGather()
    {
        if (RequiredForGatherItem != null)
        {
            bool hasReq = false;
            if (Player.Instance.CurrentFollower != null)
            {
                hasReq = Player.Instance.CurrentFollower.Inventory.HasItem(RequiredForGatherItem);
            }
            if (hasReq == false)
            {
                hasReq = Player.Instance.Inventory.HasItem(RequiredForGatherItem);
            }
            if (hasReq == false)
            {
                CurrentGatherItem = null;
                MessageManager.AddMessage("You have run out of " + RequiredForGatherItem);
                RequiredForGatherItem = null;
                return false;
            }
            else
            {
                Player.Instance.Inventory.RemoveItems(RequiredForGatherItem, 1);
            }
        }
        return true;
    }
    private void ReadBook()
    {
        if(CurrentBook != null)
        {
            Player.Instance.GainExperience(CurrentBook.Skill, (long)((Player.Instance.GetLevel(CurrentBook.Skill.Name) / 80d) * 500));
            MessageManager.AddMessage("You read another page of the book. You feel more knowledgable about " + CurrentBook.Skill.Name + ".");
            CurrentBook.Progress++;
            TicksToNextAction = (int)Math.Max(100, ((2 + CurrentBook.Difficulty * 5d) / (Player.Instance.GetLevel(CurrentBook.Skill.Name))) * 100);
            if (Random.Next(0, CurrentBook.Length) == CurrentBook.Progress)
            {
                MessageManager.AddMessage("A small key falls out of the book as you turn the page.");
                if(BGColor == "#2e1b1b")
                {
                    Player.Instance.Inventory.AddItem("Small Red Library Key");
                }
                else if(BGColor == "#463513")
                {
                    Player.Instance.Inventory.AddItem("Small Orange Library Key");
                }
                else
                {
                    Player.Instance.Inventory.AddItem("Small Green Library Key");
                }
                
            }
            if(CurrentBook.Progress >= CurrentBook.Length)
            {
                MessageManager.AddMessage("You finish reading the book and return it to the shelf.");
                CurrentBook = null;

            }
        }
    }
    private void CraftItem()
    {
        if (CurrentRecipe == null)
        {
            return;
        }
        else if(BattleManager.Instance.BattleHasEnded == false)
        {
            MessageManager.AddMessage("You can't make that while fighting!");
            CurrentRecipe = null;
            return;
        }
        if (CurrentRecipe.Create(out int created))
        {
            TicksToNextAction = CurrentRecipe.CraftingSpeed;
            MessageManager.AddMessage(CurrentRecipe.GetOutputsString().Replace("$", (created * CurrentRecipe.OutputAmount).ToString()));
            if(CurrentRecipe.CanCreate() == false)
            {
                
                if (CurrentRecipe.HasSpace())
                {
                    CurrentRecipe.Output.Rerender = true;
                    MessageManager.AddMessage("You have run out of materials.");
                    if(OvenComponent != null)
                    {
                        OvenComponent.Source = "";
                    }
                }
                else
                {
                    MessageManager.AddMessage("You don't have enough inventory space to do it again.");
                }
                if (CurrentRecipe != null)
                {
                    foreach (Ingredient i in CurrentRecipe.Ingredients)
                    {
                        i.Item.Rerender = true;
                    }
                }
                CurrentRecipe = null;
                itemViewer.ClearItem();
            }
            if(CurrentRecipe != null)
            {
                foreach (Ingredient i in CurrentRecipe.Ingredients)
                {
                    i.Item.Rerender = true;
                }
            }

        }
        else
        {
            if(CurrentRecipe.Ingredients.Count == 1 && CurrentRecipe.Ingredients[0].Item.Name == itemViewer.Item.Name && CurrentRecipe.HasSpace())
            {
                itemViewer.ClearItem();                
                MessageManager.AddMessage("You have run out of materials.");
                CurrentRecipe.Output.Rerender = true;
                if (OvenComponent != null)
                {
                    OvenComponent.Source = "";
                }
            }
            foreach(Ingredient i in CurrentRecipe.Ingredients)
            {
                i.Item.Rerender = true;
            }
            CurrentRecipe = null;
        }
        if(CraftingComponent != null)
        {
            CraftingComponent.ReloadRecipes();
        }
        

    }
    public void Eat(GameItem item)
    {
        if (item.FoodInfo != null)
        {
            CurrentFood = item;
            HealingTicks = CurrentFood.FoodInfo.HealDuration;
            Player.Instance.Inventory.RemoveItems(item, 1);
            if (CurrentFood.FoodInfo.BuffedSkill != null)
            {
                Player.Instance.Skills.Find(x => x.Name == item.FoodInfo.BuffedSkill).Boost = CurrentFood.FoodInfo.BuffAmount;
                MessageManager.AddMessage("You eat a " + CurrentFood + "." + " It somehow makes you feel better at " + CurrentFood.FoodInfo.BuffedSkill + ".");

            }
            else
            {
                MessageManager.AddMessage("You eat a " + CurrentFood + ".");

            }
        }
        else
        {
            Console.WriteLine("Item has no food info:" + item.Name);
        }


    }
    private void HealPlayer()
    {

        Player.Instance.CurrentHP = Math.Min(Player.Instance.MaxHP, Player.Instance.CurrentHP + CurrentFood.FoodInfo.HealAmount);
        if (Player.Instance.CurrentHP <= 0)
        {
            Player.Instance.Die();
        }
        HealingTicks--;
        if (HealingTicks <= 0)
        {
            if (CurrentFood.FoodInfo.BuffedSkill != null)
            {
                Player.Instance.Skills.Find(x => x.Name == CurrentFood.FoodInfo.BuffedSkill).Boost = 0;
            }
            CurrentFood = null;
        }
    }

    private void SmithItem()
    {
        if(CurrentSmeltingRecipe == null || CurrentSmithingRecipe == null)
        {
            Console.WriteLine("Failed to start smithing, Smelting Item is null:" + (CurrentSmeltingRecipe == null) + " and Smithing Item is null:" + (CurrentSmithingRecipe == null));
            return;
        }
        if(SmithingManager.SmithingStage == 0)
        {
            if (Player.Instance.CurrentFollower != null && Player.Instance.CurrentFollower.AutoCollectSkill == "Smithing")
            {
                SmithingManager.AutoSmithedItemCount = 0;
                SmithingManager.GetAutoSmeltingMaterials(CurrentSmeltingRecipe);
            }
            else if (SmithingManager.DoSmelting(CurrentSmeltingRecipe))
            {
                TicksToNextAction = CurrentSmeltingRecipe.CraftingSpeed;
            }
            else
            {
                if (SmithingComponent != null)
                {
                    SmithingComponent.UpdateSmeltables();
                }
                MessageManager.AddMessage("You have run out of ores.");
                StopActions();
                SmithingManager.SmithingStage = 0;
            }
        }
        else if(SmithingManager.SmithingStage == 1)
        {
            if (Player.Instance.CurrentFollower != null && Player.Instance.CurrentFollower.AutoCollectSkill == "Smithing")
            {
                SmithingManager.DoAutoSmelting(CurrentSmeltingRecipe, CurrentSmithingRecipe);
            }
            else if (SmithingManager.DoSmithing(CurrentSmeltingRecipe, CurrentSmithingRecipe))
            {
                TicksToNextAction = CurrentSmithingRecipe.CraftingSpeed;
            }
            else
            {
                if (SmithingComponent != null)
                {
                    SmithingComponent.UpdateSmeltables();
                }
                StopActions();
            }
        }
        else if(SmithingManager.SmithingStage == 2)
        {
            if (Player.Instance.CurrentFollower != null && Player.Instance.CurrentFollower.AutoCollectSkill == "Smithing")
            {
                SmithingManager.DoAutoSmithing(CurrentSmithingRecipe);
            }
            else if (Player.Instance.Inventory.AddMultipleOfItem(CurrentSmithingRecipe.Output, CurrentSmithingRecipe.OutputAmount))
            {
                MessageManager.AddMessage("You withdraw " + CurrentSmithingRecipe.OutputAmount + " " + CurrentSmithingRecipe.Output.Name);
                Player.Instance.GainExperience(CurrentSmithingRecipe.ExperienceGained);
                if(CurrentArtisanTask != null)
                {
                    if(CurrentArtisanTask.ItemName == CurrentSmithingRecipe.OutputItemName)
                    {
                        if (long.TryParse(CurrentSmithingRecipe.ExperienceGained.Split(':')[1], out long xp))
                        {
                            Player.Instance.GainExperience("Artisan", xp / 5);
                        }
                        else
                        {
                            Player.Instance.GainExperience("Artisan", 15);
                        }
                    }
                }
                TicksToNextAction = 12;
                SmithingManager.SmithingStage = 0;
            }
        }
        else if(SmithingManager.SmithingStage == 3)
        {
            if (Player.Instance.CurrentFollower != null && Player.Instance.CurrentFollower.AutoCollectSkill == "Smithing")
            {
                SmithingManager.DoAutoWithdrawal(CurrentSmithingRecipe);
            }
        }
        else if(SmithingManager.SmithingStage == 4)
        {
            if (Player.Instance.CurrentFollower != null && Player.Instance.CurrentFollower.AutoCollectSkill == "Smithing")
            {
                SmithingManager.DepositOutputs();
            }
        }

    }

    public void SellItem(GameItem item)
    {
        if (item.IsSellable == false)
        {
            MessageManager.AddMessage("You can't seem to sell this...");
            return;
        }
        if (item.IsEquipped)
        {
            MessageManager.AddMessage("You need to unequip this item before selling it.");
            return;
        }
        if (ItemManager.Instance.CurrentShop != null)
        {
            int sellAmt = Math.Min(ItemManager.Instance.SellAmount, Player.Instance.Inventory.GetNumberOfUnlockedItem(item));
            if (Player.Instance.Inventory.GetAvailableSpaces() > 0 || Player.Instance.Inventory.GetNumberOfItem(ItemManager.Instance.CurrentShop.Currency) > 0 || (item.IsStackable == false))
            {
                int amtRemoved = Player.Instance.Inventory.RemoveUnlockedItems(item, sellAmt);
                Player.Instance.Inventory.AddMultipleOfItem(ItemManager.Instance.CurrentShop.Currency, (amtRemoved * item.Value / 2));
                MessageManager.AddMessage("You sold " + amtRemoved + " " + item.Name + " for " + (amtRemoved * item.Value / 2) + " " + ItemManager.Instance.CurrentShop.Currency.Name + ".");
            }
            else
            {
                if(Player.Instance.Inventory.GetNumberOfUnlockedItem(item) == 0)
                {
                    MessageManager.AddMessage("You dont have any unlocked " + item.Name + ".");

                }
                else
                {
                    MessageManager.AddMessage("You sold dont have the inventory space to sell that.");

                }
            }
        }
        else if(Bank.Instance.IsBanking && Player.Instance.CurrentFollower != null && Player.Instance.CurrentFollower.AutoCollectSkill == "Banking")
        {
            Console.WriteLine("Code was run!");
            int sellAmt = Math.Min(Bank.Instance.Amount, Player.Instance.Inventory.GetNumberOfUnlockedItem(item));
            if (Player.Instance.Inventory.GetAvailableSpaces() > 0 || Player.Instance.Inventory.GetNumberOfItem(ItemManager.Instance.GetItemByName("Coins")) > 0 || (item.IsStackable == false))
            {
                Player.Instance.Inventory.RemoveItems(item, sellAmt);
                Player.Instance.Inventory.AddMultipleOfItem("Coins", (sellAmt * item.Value / 2));
                MessageManager.AddMessage("You sold " + sellAmt + " " + item.Name + " for " + (sellAmt * item.Value / 2) + " Coins.");
            }
            else
            {
                if (Player.Instance.Inventory.GetNumberOfUnlockedItem(item) == 0)
                {
                    MessageManager.AddMessage("You dont have any unlocked " + item.Name + ".");

                }
                else
                {
                    MessageManager.AddMessage("You sold dont have the inventory space to sell that.");

                }
            }
        }
        else
        {
            Console.WriteLine("Shop was null");
        }
    }
    public void SellItemFromBank(GameItem item)
    {
        if (item.IsSellable == false)
        {
            MessageManager.AddMessage("You can't seem to sell this...");
            return;
        }
        if (item.IsEquipped)
        {
            MessageManager.AddMessage("You need to unequip this item before selling it.");
            return;
        }
        if (Bank.Instance.IsBanking && Player.Instance.CurrentFollower != null && Player.Instance.CurrentFollower.AutoCollectSkill == "Banking")
        {
            int sellAmt = Math.Min(Bank.Instance.Amount, Bank.Instance.Inventory.GetNumberOfItem(item));

                Bank.Instance.Inventory.RemoveItems(item, sellAmt);
                Bank.Instance.Inventory.AddMultipleOfItem("Coins", (sellAmt * item.Value / 2));
                MessageManager.AddMessage(Player.Instance.CurrentFollower.Name + " sold " + sellAmt + " " + item.Name + " for " + (sellAmt * item.Value / 2) + " Coins and deposited them into your bank.");
        }
        else
        {
            Console.WriteLine("Shop was null");
        }
        Bank.Instance.HasChanged = true;
    }
    private void AlchItem()
    {
        if(AlchemyStage == 0)
        {
            if (Player.Instance.Inventory.HasItem(CurrentAlchemyFormula.InputMetal))
            {
                Player.Instance.Inventory.RemoveItems(CurrentAlchemyFormula.InputMetal, 1);
                AlchemyStage = 1;
                TicksToNextAction = 10;
                MessageManager.AddMessage("You place the " + CurrentAlchemyFormula.InputMetal.Name + " into the pool.");
            }
            else
            {
                if(AlchemicalHallComponent != null)
                {
                    AlchemicalHallComponent.UpdateLists();
                }
                MessageManager.AddMessage("You don't have any " + CurrentAlchemyFormula.InputMetal.Name + "s.");
                CurrentAlchemyFormula = null;
                AlchemyStage = 0;
            }
        }
        else if(AlchemyStage == 1)
        {
            if (Player.Instance.Inventory.HasItem(CurrentAlchemyFormula.Element))
            {
                Player.Instance.Inventory.RemoveItems(CurrentAlchemyFormula.Element, 1);
                AlchemyStage = 2;
                TicksToNextAction = 10;
                MessageManager.AddMessage("You apply the " + CurrentAlchemyFormula.Element.Name + " to the metal mixture in the pool.");
            }
            else
            {
                if (AlchemicalHallComponent != null)
                {
                    AlchemicalHallComponent.UpdateLists();
                }
                MessageManager.AddMessage("You don't have any " + CurrentAlchemyFormula.Element.Name + ".");
                CurrentAlchemyFormula = null;
                AlchemyStage = 0;
            }
        }
        else if(AlchemyStage == 2)
        {
            MessageManager.AddMessage("You " + CurrentAlchemyFormula.ActionString + " the combined element and metal.");
            AlchemyStage = 3;
            TicksToNextAction = 10;
            if (AlchemicalHallComponent != null)
            {
                AlchemicalHallComponent.UpdateLists();
            }
        }
        else if(AlchemyStage == 3)
        {
            GameItem reward = ItemManager.Instance.GetItemFromFormula(CurrentAlchemyFormula).Copy();
            if(reward == null)
            {
                reward = ItemManager.Instance.GetItemByName("Alchemical Dust");
            }
            MessageManager.AddMessage("You withdraw the " + reward.Name + " from the release valve.");
            Player.Instance.Inventory.AddItem(reward);
            if(reward.Name == "Alchemical Dust")
            {
                StopActions();
            }
            AlchemyStage = 0;
            TicksToNextAction = 10;
            if (AlchemicalHallComponent != null)
            {
                AlchemicalHallComponent.UpdateLists();
            }
        }
    }
    public void CompleteArtisanTask()
    {
        try
        {
            Recipe r = ItemManager.Instance.GetArtisanRecipeByOutput(CurrentArtisanTask.ItemName);
            string skill = r.ExperienceGained.Split(',')[0].Split(':')[0];
            long xp = long.Parse(r.ExperienceGained.Split(',')[0].Split(':')[1]);
            Player.Instance.GainExperience(skill, xp * CurrentArtisanTask.AmountRequired / 10);
            Player.Instance.ArtisanPoints += CurrentArtisanTask.PointsToEarn;
            MessageManager.AddMessage("You completed your artisan task! You've earned " + CurrentArtisanTask.PointsToEarn + " artisan points with the guild and now have a total of " + Player.Instance.ArtisanPoints + ". Speak to a guild member to get another task.");          
        }
        catch(Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
        }
        CurrentArtisanTask = null;
    }
    public void SetCraftingItem(Recipe recipe)
    {
        StopActions();
        NewCraftingRecipe = recipe;
        TicksToNextAction = recipe.CraftingSpeed;
        MessageManager.AddMessage(recipe.RecipeActionString);
        UpdateState();
    }
    public int GetTicksToNextGather()
    {
        if(CurrentGatherItem != null)
        {
            int baseValue = CurrentGatherItem.GatherSpeed.ToGaussianRandom();
            //Console.WriteLine("Ticks to next gather:" + baseValue);
            baseValue = (int)Math.Max(1, (double)baseValue * Player.Instance.GetGearMultiplier(CurrentGatherItem));
           // Console.WriteLine("Ticks to next gather with gear:" + baseValue);
            baseValue = (int)Math.Max(1, (double)baseValue * Player.Instance.GetLevelMultiplier(CurrentGatherItem));
            //Console.WriteLine("Ticks to next gather with gear and level:" + baseValue);
            return baseValue;
        }
        return int.MaxValue;
       
    }
    public int GetTicksToNextGather(GameItem item, int gatherSpeed)
    {
        if (item != null)
        {
            int baseValue = gatherSpeed.ToGaussianRandom();
            // Console.WriteLine("Ticks to next gather:" + baseValue);
            baseValue = (int)Math.Max(1, (double)baseValue * Player.Instance.GetGearMultiplier(item));
            //Console.WriteLine("Ticks to next gather with gear:" + baseValue);
            baseValue = (int)Math.Max(1, (double)baseValue * Player.Instance.GetLevelMultiplier(item));
            //Console.WriteLine("Ticks to next gather with gear and level:" + baseValue);
            return baseValue;
        }
        return int.MaxValue;

    }
    public void CancelHuntingTrip()
    {
        foreach(Area a in AreaManager.Instance.Areas)
        {
            if(a.HuntingTripInfo != null && a.HuntingTripInfo.IsActive)
            {
                double amountCompleted = DateTime.UtcNow.Subtract(a.HuntingTripInfo.StartTime).TotalHours / a.HuntingTripInfo.ReturnTime.Subtract(a.HuntingTripInfo.StartTime).TotalHours;
                HuntingManager.EndHunt(a.HuntingTripInfo, amountCompleted, false);
            }
        }
        IsOnHuntingTrip = false;
    }
    public void ShowTooltip(MouseEventArgs args, string tipName, bool alignRight)
    {
        if (alignRight)
        {
            TooltipManager.ShowTip(args, tipName, alignRight);
            UpdateState();
        }
        else
        {
            ShowTooltip(args, tipName);
        }
    }
    public void ShowTooltip(MouseEventArgs args, string tipName, bool alignRight, bool showAbove)
    {
        TooltipManager.ShowTip(args, tipName, alignRight, showAbove);
        UpdateState();
    }
    public async Task LoadData(System.Net.Http.HttpClient Http, NavigationManager URIHelper, IJSRuntime Jsruntime)
    {
        await AreaManager.Instance.LoadAreas(Http);
        CurrentLand = AreaManager.Instance.GetLandByName("Quepland");
        UriHelper = URIHelper;
        Updates = await Http.GetFromJsonAsync<List<Update>>("data/Updates.json");
        await ItemManager.Instance.LoadItems(Http);
        await Player.Instance.LoadSkills(Http);
        await NPCManager.Instance.LoadNPCs(Http);
        await QuestManager.Instance.LoadQuests(Http);
        await BattleManager.Instance.LoadMonsters(Http);
        await FollowerManager.Instance.LoadFollowers(Http);
        JSRuntime = Jsruntime;
        await GetDimensions();
        Start();
        InitCompleted = true;
    }
    public async Task<int> GetLoadingProgress()
    {
        return GameLoadProgress;
    }
    public void ShowTooltip(MouseEventArgs args, string tipName, string tipData)
    {
        TooltipManager.ShowTip(args, tipName, tipData);
        //UpdateState();
    }
    public void ShowTooltip(MouseEventArgs args, string tipName)
    {
        TooltipManager.ShowTip(args, tipName);
        //UpdateState();
    }
    public void ShowTooltip(MouseEventArgs args, Tooltip tip)
    {
        TooltipManager.ShowTip(args, tip);
        //UpdateState();
    }
    public void ShowItemTooltip(MouseEventArgs args, string itemName, string itemDesc)
    {
        TooltipManager.ShowItemTip(args, itemName, itemDesc);
        //UpdateState();
    }
    public void ShowCraftingTooltip(MouseEventArgs args, string itemName, string itemDesc)
    {
        TooltipManager.ShowCraftingTip(args, itemName, itemDesc);
        //UpdateState();
    }
    public void ShowContextMenu(MouseEventArgs args)
    {
        if(CurrentContextMenu != null && CurrentContextMenu.Buttons.Count > 0)
        {
            TooltipManager.xPos = args.ClientX;
            TooltipManager.yPos = args.ClientY;
            TooltipManager.ShowContextMenu(args);
        }

    }
    public static void GoTo(string url)
    {
        if (url.Contains("World/"))
        {
            Location = url.Split("World/")[1];
        }
        else
        {
            Location = url;
        }
         
        UriHelper.NavigateTo(url);
    }
    public async Task GetDimensions()
    {
        GameWindowWidth = await JSRuntime.InvokeAsync<int>("getWidth");
        GameWindowHeight = await JSRuntime.InvokeAsync<int>("getHeight");
    }

    public void HideTooltip()
    {
        TooltipManager.HideTip();
    }
    public static GameStateSaveData GetSaveData()
    {
        return new GameStateSaveData { IsHunting = IsOnHuntingTrip, 
            Location = Location, 
            CurrentLand = CurrentLand.Name, 
            CurrentTask = CurrentArtisanTask,
            HideLockedItems = HideLockedItems,
            CompactInventory = CompactInventoryView
        };
    }
    public static void LoadSaveData(GameStateSaveData data)
    {
        IsOnHuntingTrip = AreaManager.LoadedHuntingInfo;
        HideLockedItems = data.HideLockedItems;
        CompactInventoryView = data.CompactInventory;

        CurrentArtisanTask = data.CurrentTask;
        if (data.Location == null || data.Location == "" || data.Location == "Battle")
        {
            Location = "QueplandFields";
            CurrentLand = AreaManager.Instance.GetLandByName("Quepland");
        }
        else
        {
            Location = data.Location;
            CurrentLand = AreaManager.Instance.GetLandByName(data.CurrentLand);
        }

        

    }
    public static void LoadAFKActionData(AFKAction action)
    {       
        if(action != null)
        {
            try
            {
                AreaManager.Instance.GetAFKActionByUniqueID(action.UniqueID).ReturnTime = action.ReturnTime;
                AreaManager.Instance.GetAFKActionByUniqueID(action.UniqueID).StartTime = action.StartTime;
                AreaManager.Instance.GetAFKActionByUniqueID(action.UniqueID).IsActive = action.IsActive;
                CurrentAFKAction = AreaManager.Instance.GetAFKActionByUniqueID(action.UniqueID);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                
            }
        }

    }
    private void StateHasChanged()
    {
        StateChanged?.Invoke(this, EventArgs.Empty);
    }
    public void UpdateState()
    {
        StateHasChanged();
    }
}

