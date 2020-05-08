using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;


public static class TooltipManager
{
    public static List<Tooltip> Tooltips = new List<Tooltip>();
    public static bool Show;
    public static Tooltip CurrentTip;
    public static double xPos;
    public static double yPos;


    public static async Task LoadTooltips(HttpClient Http)
    {
        Tooltips.AddRange(await Http.GetJsonAsync<Tooltip[]>("data/Tooltips.json"));
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
        Show = true;
        xPos = args.ClientX;
        yPos = args.ClientY;
        CurrentTip = tip;
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

