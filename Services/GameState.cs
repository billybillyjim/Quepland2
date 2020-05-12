using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
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
    public static int TicksToNextGather;
    public static int GameWindowWidth = 1200;

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
            if(TicksToNextGather <= 0 && CurrentGatherItem != null)
            {
                GatherItem();
            }
            TicksToNextGather--;
            StateHasChanged();
        }), null, 200, 200);
        StateHasChanged();
    }
    public void StopActions()
    {
        stopActions = true;
    }
    private void ClearActions()
    {
        CurrentGatherItem = null;
        stopActions = false;
    }
    public void Pause()
    {
        GameTimer.Change(Timeout.Infinite, Timeout.Infinite);
    }

    private void GatherItem()
    {
        Player.Instance.GainExperience(CurrentGatherItem.ExperienceGained);
        if (Player.Instance.Inventory.AddItem(CurrentGatherItem) == false)
        {
            CurrentGatherItem = null;
        }
        else
        {
            TicksToNextGather = CurrentGatherItem.GatherSpeed.ToGaussianRandom();
            MessageManager.AddMessage(CurrentGatherItem.GatherString);
        }
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

