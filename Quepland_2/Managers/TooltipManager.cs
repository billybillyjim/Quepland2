using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;


public static class TooltipManager
{
    public static List<Tooltip> Tooltips = new List<Tooltip>();
    public static bool Show;
    public static bool ShowContext { get; set; }
    public static Tooltip CurrentTip;
    public static double xPos;
    public static double yPos;
    public static readonly int delay = 2;
    public static int currentDelay = 0;


    public static async Task LoadTooltips(HttpClient Http)
    {
        Tooltips.AddRange(await Http.GetFromJsonAsync<Tooltip[]>("data/Tooltips.json"));
    }

    public static void ShowTip(MouseEventArgs args, string tipName, bool alignRight, bool showAbove)
    {
        ShowTip(args, tipName);

        CurrentTip.RightAlignData = alignRight;
        CurrentTip.ShowAbove = showAbove;
        
    }

    public static void ShowTip(MouseEventArgs args, string tipName, bool alignRight)
    {
        ShowTip(args, tipName);
        if (alignRight)
        {
            CurrentTip.RightAlignData = true;
        }
    }
    public static void ShowTip(MouseEventArgs args, string tipName, string tipData)
    {
        Tooltip tip = GetTooltipByName(tipName);
        if (tip == null)
        {
            tip = new Tooltip("", tipName, tipData);
        }
        ShowTip(args, tip);
    }
    public static void ShowTip(MouseEventArgs args, string tipName)
    {
        Tooltip tip = GetTooltipByName(tipName);
        if (tip == null)
        {
            tip = new Tooltip("", "", tipName);
        }
        ShowTip(args, tip);
    }
    public static void ShowTip(MouseEventArgs args, Tooltip tip)
    {
        if (!ShowContext && CurrentTip != tip)
        {
            Show = true;
            xPos = args.ClientX;
            yPos = args.ClientY;
            CurrentTip = tip;
            ShowContext = false;
            
        }
        currentDelay = 0;
    }
    public static void ShowItemTip(MouseEventArgs args, string name, string desc)
    {
        if (!ShowContext)
        {
            Show = true;
            xPos = args.ClientX;
            yPos = args.ClientY;
            if(CurrentTip != null)
            {
                CurrentTip.Title = name;
                CurrentTip.Text = desc;
            }
            else
            {
                CurrentTip = new Tooltip(name, name,desc);
            }
            ShowContext = false;
        }
        currentDelay = 0;
    }
    public static void ShowCraftingTip(MouseEventArgs args, string name, string desc)
    {
        bool isShowing = false;
        if (!ShowContext)
        {
            Show = true;
            xPos = args.ClientX;
            yPos = args.ClientY;
           
            if (CurrentTip != null)
            {
                if(CurrentTip.Name == name)
                {
                    isShowing = true;
                }
                CurrentTip.Title = name;
                CurrentTip.Text = desc;
            }
            else
            {
                CurrentTip = new Tooltip(name, name, desc);
            }
            ShowContext = false;
        }
        if(isShowing == false)
        {
            currentDelay = 0;
        }
    }
    public static void ShowContextMenu(MouseEventArgs args)
    {
        Show = false;
        ShowContext = true;
        xPos = args.ClientX;
        yPos = args.ClientY;
    }
    public static bool ShouldShow()
    {
        if(currentDelay >= delay && Show)
        {
            return true;
        }
        return false;
    } 
    public static void HideTip()
    {
        Show = false;
    }
    public static Tooltip GetTooltipByName(string name)
    {
        return Tooltips.Find(x => x.Name == name);
    }

}

