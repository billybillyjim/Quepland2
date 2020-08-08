using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Quepland_2.Components;
using Quepland_2.Pages;
using Quepland_2.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


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

    public static string Version { get; set; } = "0.0.9";

    public static string Location { get; set; } = "";
    public static bool InitCompleted { get; set; } = false;
    public static bool ShowStartMenu { get; set; } = true;
    public static bool ShowSettings { get; set; }
    private bool stopActions = false;
    private bool stopNoncombatActions = false;
    public bool IsStoppingNextTick { get; set; }
    public static bool IsOnHuntingTrip { get; set; } 
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
    public Recipe NewCraftingRecipe;
    public Recipe NewSmeltingRecipe;
    public Recipe NewSmithingRecipe;

    public GameItem CurrentGatherItem;
    public List<GameItem> PossibleGatherItems = new List<GameItem>();
    public Recipe CurrentSmeltingRecipe;
    public Recipe CurrentSmithingRecipe;

    public AlchemicalFormula CurrentAlchemyFormula;
    
    public GameItem CurrentFood;
    public Recipe CurrentRecipe;
    public static Land CurrentLand;
    public ItemViewerComponent itemViewer;
    public InventoryViewerComponent inventoryViewer;
    public SmithyComponent SmithingComponent;
    public OvenComponent OvenComponent;
    public NavMenu NavMenu;
    public ContextMenu CurrentContextMenu;
    public ExperienceTrackerComponent EXPTrackerComponent;
    public static NavigationManager UriHelper;
    public static int TicksToNextAction;
    public static readonly int GameSpeed = 200;
    public int TicksToNextHeal;
    public int HealingTicks;
    public static int CurrentTick { get; set; }
    public static int AutoSaveInterval { get; set; } = 9000;
    public static int GameWindowWidth { get; set; }
    public static int GameWindowHeight { get; set; }
    public int MinWindowWidth { get; set; } = 600;
    public int SmithingStage;
    public int AlchemyStage;
    private int AutoSmithedItemCount { get; set; } = 0;

    private QuestTester QuestTester = new QuestTester();
    private RecipeTester RecipeTester = new RecipeTester();
    private static Random Random = new Random();

    public bool SaveGame = false;
    public static bool IsSaving = false;

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
        if (IsOnHuntingTrip)
        {
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
            HealPlayer();
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
            Player.Instance.JustDied = false;
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
        CurrentRecipe = null;
        BattleManager.Instance.CurrentOpponents.Clear();
        CurrentSmithingRecipe = null;
        CurrentSmeltingRecipe = null;
        stopActions = false;
        IsStoppingNextTick = false;
        if(NewGatherItem != null)
        {
            CurrentGatherItem = NewGatherItem;
            NewGatherItem = null;
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
        if (Player.Instance.FollowerGatherItem(CurrentGatherItem) == false)
        {
            PlayerGatherItem();
        }
        if(CurrentGatherItem != null)
        {
            if (Player.Instance.CurrentFollower != null && Player.Instance.CurrentFollower.IsBanking)
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
    private bool PlayerGatherItem()
    {
        if (Player.Instance.PlayerGatherItem(CurrentGatherItem) == false)
        {
            if(Player.Instance.CurrentFollower != null)
            {
                if (Player.Instance.CurrentFollower.IsBanking == false)
                {
                    CurrentGatherItem = null;
                    return false;
                }
                else
                {
                    TicksToNextAction = Player.Instance.CurrentFollower.TicksToNextAction;
                }
            }

        }
        return true;
        
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
            MessageManager.AddMessage(CurrentRecipe.GetOutputsString().Replace("$", created.ToString()));
            if(CurrentRecipe.CanCreate() == false)
            {
                
                if (CurrentRecipe.HasSpace())
                {
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
                CurrentRecipe = null;
                itemViewer.ClearItem();
            }
        }
        else
        {
            if(CurrentRecipe.Ingredients.Count == 1 && CurrentRecipe.Ingredients[0].Item.Name == itemViewer.Item.Name && CurrentRecipe.HasSpace())
            {
                itemViewer.ClearItem();
                MessageManager.AddMessage("You have run out of materials.");
                if (OvenComponent != null)
                {
                    OvenComponent.Source = "";
                }
            }
           
            CurrentRecipe = null;
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
        if(SmithingStage == 0)
        {
            if(Player.Instance.CurrentFollower != null && Player.Instance.CurrentFollower.AutoCollectSkill == "Smithing")
            {
                GetAutoSmeltingMaterials();
            }
            else if(DoSmelting() == false)
            {
                if (SmithingComponent != null)
                {
                    SmithingComponent.UpdateSmeltables();
                }
                MessageManager.AddMessage("You have run out of ores.");
                StopActions();               
            }
        }
        else if(SmithingStage == 1)
        {
            if (Player.Instance.CurrentFollower != null && Player.Instance.CurrentFollower.AutoCollectSkill == "Smithing")
            {
                DoAutoSmelting();
            }
            else if (DoSmithing() == false)
            {
                if (SmithingComponent != null)
                {
                    SmithingComponent.UpdateSmeltables();
                }
                StopActions();
            }
        }
        else if(SmithingStage == 2)
        {
            if (Player.Instance.CurrentFollower != null && Player.Instance.CurrentFollower.AutoCollectSkill == "Smithing")
            {
                DoAutoSmithing();
            }
            else if (Player.Instance.Inventory.AddMultipleOfItem(CurrentSmithingRecipe.Output, CurrentSmithingRecipe.OutputAmount))
            {
                MessageManager.AddMessage("You withdraw " + CurrentSmithingRecipe.OutputAmount + " " + CurrentSmithingRecipe.Output.Name);
                TicksToNextAction = 12;
                SmithingStage = 0;
            }
        }
        else if(SmithingStage == 3)
        {
            if (Player.Instance.CurrentFollower != null && Player.Instance.CurrentFollower.AutoCollectSkill == "Smithing")
            {
                DoAutoWithdrawal();
            }
        }
        else if(SmithingStage == 4)
        {
            if (Player.Instance.CurrentFollower != null && Player.Instance.CurrentFollower.AutoCollectSkill == "Smithing")
            {
                DepositOutputs();
            }
        }

    }
    private bool DoSmelting()
    {
        if (Player.Instance.Inventory.RemoveRecipeItems(CurrentSmeltingRecipe))
        {
            MessageManager.AddMessage("You smelt the " + CurrentSmeltingRecipe.GetIngredientsOnlyString() + " into a " + CurrentSmeltingRecipe.OutputItemName);
            Player.Instance.Inventory.AddItem(CurrentSmeltingRecipe.Output);
            //Player.Instance.GainExperience("Smithing", CurrentSmeltingItem.SmithingInfo.SmeltingExperience);
            TicksToNextAction = CurrentSmeltingRecipe.CraftingSpeed;
            SmithingStage = 1;
            return true;
        }
        return false;
    }
    private bool GetAutoSmeltingMaterials()
    {
        if(Player.Instance.CurrentFollower != null && Player.Instance.CurrentFollower.AutoCollectSkill == "Smithing")
        {
            if(Player.Instance.CurrentFollower.TicksToNextAction <= 0)
            {
                if (Player.Instance.CurrentFollower.Inventory.GetUsedSpaces() == 0)
                {
                    int amtToWithdraw = Player.Instance.CurrentFollower.InventorySize / CurrentSmeltingRecipe.GetNumberOfIngredients();
                    foreach (Ingredient i in CurrentSmeltingRecipe.Ingredients)
                    {
                        int actualAmt = Math.Min(amtToWithdraw, Bank.Instance.Inventory.GetNumberOfItem(i.Item));
                        if (actualAmt == 0)
                        {
                            
                            return false;
                        }
                        Bank.Instance.Inventory.RemoveItems(i.Item, actualAmt);
                        Player.Instance.CurrentFollower.Inventory.AddMultipleOfItem(i.Item, actualAmt);
                        Player.Instance.CurrentFollower.TicksToNextAction = Player.Instance.CurrentFollower.AutoCollectSpeed;
                    }
                    MessageManager.AddMessage(Player.Instance.CurrentFollower.Name + " goes to the bank and gathers the resources to smith.");
                    SmithingStage++;
                    return true;
                }
            }
            else
            {
                Player.Instance.CurrentFollower.TicksToNextAction--;
                return true;
            }
        }
        return false;
    }
    private void DoAutoSmelting()
    {
        if (Player.Instance.CurrentFollower != null && Player.Instance.CurrentFollower.AutoCollectSkill == "Smithing")
        {
            if (Player.Instance.CurrentFollower.TicksToNextAction <= 0)
            {
                if (Player.Instance.CurrentFollower.Inventory.RemoveRecipeItemsFromFollower(CurrentSmeltingRecipe))
                {
                    MessageManager.AddMessage(Player.Instance.CurrentFollower.Name + " helps smelt the " + CurrentSmeltingRecipe.Output + ".");
                    Player.Instance.CurrentFollower.Inventory.AddItem(CurrentSmeltingRecipe.Output);
                    Player.Instance.CurrentFollower.TicksToNextAction = Player.Instance.CurrentFollower.AutoCollectSpeed;
                    AutoSmithedItemCount += CurrentSmithingRecipe.OutputAmount;
                }
                else
                {
                    MessageManager.AddMessage("Everything is ready, so you prepare to hammer the metal.");
                    SmithingStage++;
                    return;
                }
            }
            else
            {
                Player.Instance.CurrentFollower.TicksToNextAction--;
            }
        }
    }
    private bool DoSmithing()
    {
        if (Player.Instance.Inventory.RemoveRecipeItems(CurrentSmithingRecipe))
        {
            MessageManager.AddMessage("You hammer the " + CurrentSmeltingRecipe.OutputItemName + " into a " + CurrentSmithingRecipe.OutputItemName + " and place it in water to cool.");
            //Player.Instance.GainExperience("Smithing", CurrentSmeltingItem.SmithingInfo.SmeltingExperience);
            TicksToNextAction = CurrentSmithingRecipe.CraftingSpeed;
            SmithingStage = 2;
            return true;
        }
        return false;
    }
    private bool DoAutoSmithing()
    {
        if (Player.Instance.CurrentFollower != null && Player.Instance.CurrentFollower.AutoCollectSkill == "Smithing")
        {
            if (Player.Instance.CurrentFollower.TicksToNextAction <= 0)
            {
                if (Player.Instance.CurrentFollower.Inventory.RemoveRecipeItemsFromFollower(CurrentSmithingRecipe))
                {
                    MessageManager.AddMessage(Player.Instance.CurrentFollower.Name + " helps hammer the heated bar into shape.");
                    Player.Instance.CurrentFollower.TicksToNextAction = Player.Instance.CurrentFollower.AutoCollectSpeed;               
                    return true;
                }
                else
                {
                    MessageManager.AddMessage("Together you finish hammering out the last " + CurrentSmithingRecipe.Output + ".");
                    SmithingStage++;
                    return true;
                }
            }
            else
            {
                Player.Instance.CurrentFollower.TicksToNextAction--;
                return true;
            }
        }
        return false;
    }
    private void DoAutoWithdrawal()
    {
        if (Player.Instance.CurrentFollower != null && Player.Instance.CurrentFollower.AutoCollectSkill == "Smithing")
        {
            if (Player.Instance.CurrentFollower.TicksToNextAction <= 0)
            {
                Player.Instance.CurrentFollower.Inventory.AddMultipleOfItem(CurrentSmithingRecipe.Output, CurrentSmithingRecipe.OutputAmount);
                AutoSmithedItemCount -= CurrentSmithingRecipe.OutputAmount;
                MessageManager.AddMessage(Player.Instance.CurrentFollower.Name + " gathers up a cooled " + CurrentSmithingRecipe.OutputItemName + ".");
                Player.Instance.CurrentFollower.TicksToNextAction = Player.Instance.CurrentFollower.AutoCollectSpeed;
                if(AutoSmithedItemCount <= 0)
                {
                    SmithingStage = 4;
                }
            }
            else
            {
                Player.Instance.CurrentFollower.TicksToNextAction--;
            }
        }
    }
    private void DepositOutputs()
    {
        if (Player.Instance.CurrentFollower != null && Player.Instance.CurrentFollower.AutoCollectSkill == "Smithing")
        {
            if (Player.Instance.CurrentFollower.TicksToNextAction <= 0)
            {
                Bank.Instance.DepositAll(Player.Instance.CurrentFollower.Inventory);
                MessageManager.AddMessage(Player.Instance.CurrentFollower.Name + " returns to the bank and deposits everything.");
                Player.Instance.CurrentFollower.TicksToNextAction = Player.Instance.CurrentFollower.AutoCollectSpeed;
                SmithingStage = 0;
            }
            else
            {
                Player.Instance.CurrentFollower.TicksToNextAction--;
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
            int sellAmt = Math.Min(ItemManager.Instance.SellAmount, Player.Instance.Inventory.GetNumberOfItem(item));
            if (Player.Instance.Inventory.GetAvailableSpaces() > 0 || Player.Instance.Inventory.GetNumberOfItem(ItemManager.Instance.CurrentShop.Currency) > 0 || (item.IsStackable == false))
            {
                Player.Instance.Inventory.RemoveItems(item, sellAmt);
                Player.Instance.Inventory.AddMultipleOfItem(ItemManager.Instance.CurrentShop.Currency, (sellAmt * item.Value / 2));
                MessageManager.AddMessage("You sold " + sellAmt + " " + item.Name + " for " + (sellAmt * item.Value / 2) + " " + ItemManager.Instance.CurrentShop.Currency.Name + ".");
            }
            else
            {
                Console.WriteLine("No inventory space.");
            }
        }
        else if(Bank.Instance.IsBanking && Player.Instance.CurrentFollower != null && Player.Instance.CurrentFollower.AutoCollectSkill == "Banking")
        {
            int sellAmt = Math.Min(Bank.Instance.Amount, Player.Instance.Inventory.GetNumberOfItem(item));
            if (Player.Instance.Inventory.GetAvailableSpaces() > 0 || Player.Instance.Inventory.GetNumberOfItem(ItemManager.Instance.GetItemByName("Coins")) > 0 || (item.IsStackable == false))
            {
                Player.Instance.Inventory.RemoveItems(item, sellAmt);
                Player.Instance.Inventory.AddMultipleOfItem("Coins", (sellAmt * item.Value / 2));
                MessageManager.AddMessage("You sold " + sellAmt + " " + item.Name + " for " + (sellAmt * item.Value / 2) + " Coins.");
            }
            else
            {
                Console.WriteLine("No inventory space.");
            }
        }
        else
        {
            Console.WriteLine("Shop was null");
        }
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

                MessageManager.AddMessage("You don't have any " + CurrentAlchemyFormula.InputMetal.Name + ".");
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
        }
        else if(AlchemyStage == 3)
        {
            GameItem reward = ItemManager.Instance.GetItemFromFormula(CurrentAlchemyFormula);
            MessageManager.AddMessage("You withdraw the " + reward.Name + " from the release valve.");
            Player.Instance.Inventory.AddItem(reward);
            AlchemyStage = 0;
            TicksToNextAction = 10;
        }
    }
    public void SetCraftingItem(Recipe recipe)
    {
        StopActions();
        NewCraftingRecipe = recipe;
        TicksToNextAction = recipe.CraftingSpeed;
        MessageManager.AddMessage(recipe.RecipeActionString);
        UpdateState();
    }
    private int GetTicksToNextGather()
    {
        if(CurrentGatherItem != null)
        {
            int baseValue = CurrentGatherItem.GatherSpeed.ToGaussianRandom();
           // Console.WriteLine("Ticks to next gather:" + baseValue);
            baseValue = (int)Math.Max(1, (double)baseValue * Player.Instance.GetGearMultiplier(CurrentGatherItem));
            //Console.WriteLine("Ticks to next gather with gear:" + baseValue);
            baseValue = (int)Math.Max(1, (double)baseValue * Player.Instance.GetLevelMultiplier(CurrentGatherItem));
            //Console.WriteLine("Ticks to next gather with gear and level:" + baseValue);
            return baseValue;
        }
        Console.WriteLine("Current Gather Item Was null.");
        return int.MaxValue;
       
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
    public void ShowTooltip(MouseEventArgs args, string tipName, string tipData)
    {
        TooltipManager.ShowTip(args, tipName, tipData);
        UpdateState();
    }
    public void ShowTooltip(MouseEventArgs args, string tipName)
    {
        TooltipManager.ShowTip(args, tipName);
        UpdateState();
    }
    public void ShowTooltip(MouseEventArgs args, Tooltip tip)
    {
        TooltipManager.ShowTip(args, tip);
        UpdateState();
    }
    public void ShowItemTooltip(MouseEventArgs args, string itemName, string itemDesc)
    {
        TooltipManager.ShowItemTip(args, itemName, itemDesc);
        UpdateState();
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
        return new GameStateSaveData { IsHunting = IsOnHuntingTrip, Location = Location, CurrentLand = CurrentLand.Name };
    }
    public static void LoadSaveData(GameStateSaveData data)
    {
        IsOnHuntingTrip = data.IsHunting;
        if (data.Location == "Battle")
        {
            data.Location = "QueplandFields";
        }
        if (data.Location == null || data.Location == "")
        {
            data.Location = "QueplandFields";
        }
        CurrentLand = AreaManager.Instance.GetLandByName(data.CurrentLand);
        GoTo("/World/" + data.Location);
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

