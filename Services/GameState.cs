using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


    public class GameState
    {
    public event EventHandler StateChanged;
    public IJSRuntime JSRuntime;

    public static string Version { get; set; } = "0.0.1";
    public static string Location { get; set; } = "";
    public static bool InitCompleted { get; set; } = false;
    public static bool ShowStartMenu { get; set; } = true;
    private bool stopActions = false;
    private bool stopNoncombatActions = false;
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
    public GameItem CurrentGatherItem;
    public GameItem CurrentSmeltingItem;
    public GameItem CurrentSmithingItem;
    public Recipe CurrentRecipe;
    public Land CurrentLand;
    public static int TicksToNextAction;
    public static int GameWindowWidth;
    public int SmithingStage;

    public void Start()
    {
        if(GameTimer != null)
        {
            return;
        }
        GameTimer = new Timer(new TimerCallback(_ =>
        {
            if (stopActions)
            {
                ClearActions();
            }
            else if (stopNoncombatActions)
            {
                ClearNonCombatActions();
            }
            if(TicksToNextAction <= 0 && CurrentGatherItem != null)
            {                
                GatherItem();
            }
            else if(TicksToNextAction <= 0 && CurrentRecipe != null)
            {
                CraftItem();
            }
            else if(TicksToNextAction <= 0 && CurrentSmithingItem != null && CurrentSmeltingItem != null)
            {
                SmithItem();
            }
            else if(BattleManager.Instance.CurrentOpponent != null)
            {
                BattleManager.Instance.DoBattle();
            }
            if(Player.Instance.CurrentFollower != null && Player.Instance.CurrentFollower.IsBanking)
            {
                Player.Instance.CurrentFollower.TicksToNextAction--;
                if(Player.Instance.CurrentFollower.TicksToNextAction <= 0)
                {
                    Player.Instance.CurrentFollower.BankItems();
                }
            }
            GetDimensions();
            TicksToNextAction--;
            StateHasChanged();
        }), null, 200, 200);
        StateHasChanged();
    }
    /// <summary>
    /// Pauses actions at the beginning of the next game tick.
    /// </summary>
    public void StopActions()
    {
        stopActions = true;
    }
    public void StopNonCombatActions()
    {
        stopNoncombatActions = true;
    }
    private void ClearActions()
    {
        CurrentGatherItem = null;
        CurrentRecipe = null;
        BattleManager.Instance.CurrentOpponent = null;
        CurrentSmithingItem = null;
        CurrentSmeltingItem = null;
        stopActions = false;
    }
    private void ClearNonCombatActions()
    {
        CurrentGatherItem = null;
        CurrentRecipe = null;
        CurrentSmithingItem = null;
        CurrentSmeltingItem = null;
        stopNoncombatActions = false;
    }
    public void Pause()
    {
        GameTimer.Change(Timeout.Infinite, Timeout.Infinite);
    }

    private void GatherItem()
    {
        if (Player.Instance.CurrentFollower != null && Player.Instance.CurrentFollower.IsBanking == false)
        {
            if (Player.Instance.CurrentFollower.Inventory.GetAvailableSpaces() <= 0)
            {
                Player.Instance.CurrentFollower.IsBanking = true;
                Player.Instance.CurrentFollower.TicksToNextAction = Player.Instance.CurrentFollower.AutoCollectSpeed;
                PlayerGatherItem();
                MessageManager.AddMessage(Player.Instance.CurrentFollower.AutoCollectMessage.Replace("$", CurrentGatherItem.Name));
            }
            else
            {
                Player.Instance.CurrentFollower.Inventory.AddItem(CurrentGatherItem);
                MessageManager.AddMessage(CurrentGatherItem.GatherString + " and hand it over to " + Player.Instance.CurrentFollower.Name + ".");
            }
            
        }
        else {
            PlayerGatherItem();
        }
        TicksToNextAction = GetTicksToNextGather();
        Player.Instance.GainExperience(CurrentGatherItem.ExperienceGained);



    }
    private bool PlayerGatherItem()
    {
        if (Player.Instance.Inventory.AddItem(CurrentGatherItem) == false)
        {
            if(Player.Instance.CurrentFollower != null && Player.Instance.CurrentFollower.IsBanking)
            {
               
            }
            else
            {
                CurrentGatherItem = null;
            }       
            return false;
        }
        else
        {            
            MessageManager.AddMessage(CurrentGatherItem.GatherString);
        }
        return true;
    }
    private void CraftItem()
    {
        if (CurrentRecipe == null)
        {
            return;
        }
        else if(BattleManager.Instance.CurrentOpponent != null)
        {
            MessageManager.AddMessage("You can't make that while fighting a " + BattleManager.Instance.CurrentOpponent.Name + "!");
            return;
        }
        if (CurrentRecipe.Create())
        {
            TicksToNextAction = CurrentRecipe.CraftingSpeed;
            MessageManager.AddMessage(CurrentRecipe.Output.GatherString);
        }
        else
        {
            MessageManager.AddMessage("You have run out of materials.");
            CurrentRecipe = null;
        }

        
    }
    private void SmithItem()
    {
        if(CurrentSmeltingItem == null || CurrentSmithingItem == null)
        {
            Console.WriteLine("Failed to start smithing, Smelting Item is null:" + (CurrentSmeltingItem == null) + " and Smithing Item is null:" + (CurrentSmithingItem == null));
            return;
        }
        if(SmithingStage == 0)
        {
            if (Player.Instance.Inventory.RemoveItems(CurrentSmeltingItem, 1) > 0)
            {
                MessageManager.AddMessage("You smelt the " + CurrentSmeltingItem.Name + " into a " + CurrentSmeltingItem.SmithingInfo.SmeltsIntoString);
                Player.Instance.Inventory.AddItem(CurrentSmeltingItem.SmithingInfo.SmeltsInto);
                Player.Instance.GainExperience("Smithing", CurrentSmeltingItem.SmithingInfo.SmeltingExperience);
                TicksToNextAction = CurrentSmeltingItem.SmithingInfo.SmithingSpeed;
                SmithingStage = 1;
            }
            else
            {
                MessageManager.AddMessage("You have run out of ores.");
                CurrentSmithingItem = null;
                CurrentSmeltingItem = null;
            }
        }
        else if(SmithingStage == 1)
        {
            if (Player.Instance.Inventory.RemoveItems(CurrentSmeltingItem.SmithingInfo.SmeltsInto, 1) > 0)
            {
                MessageManager.AddMessage("You hammer the " + CurrentSmeltingItem.SmithingInfo.SmeltsInto.Name + " into a " + CurrentSmithingItem.Name + " and place it in water to cool.");              
                Player.Instance.GainExperience("Smithing", CurrentSmeltingItem.SmithingInfo.SmeltingExperience);
                TicksToNextAction = CurrentSmeltingItem.SmithingInfo.SmithingSpeed;
                SmithingStage = 2;
            }
        }
        else if(SmithingStage == 2)
        {
            if (Player.Instance.Inventory.AddItem(CurrentSmithingItem))
            {
                MessageManager.AddMessage("You withdraw the " + CurrentSmithingItem.Name);
                TicksToNextAction = CurrentSmeltingItem.SmithingInfo.SmeltingSpeed;
                SmithingStage = 0;
            }
        }

    }
    private int GetTicksToNextGather()
    {
        int baseValue = CurrentGatherItem.GatherSpeed.ToGaussianRandom();
        Console.WriteLine("Base Value:" + baseValue);
        baseValue = (int)Math.Max(1, (double)baseValue * Player.Instance.GetGearMultiplier(CurrentGatherItem));
        Console.WriteLine("Modified Base Value:" + baseValue);
        return baseValue;
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
    public async Task GetDimensions()
    {
        GameWindowWidth = await JSRuntime.InvokeAsync<int>("getWidth");
    }
    public void HideTooltip()
    {
        TooltipManager.HideTip();
        UpdateState();
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

