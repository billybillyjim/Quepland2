using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;


public class NPCManager
{
    private static readonly NPCManager instance = new NPCManager();
    private NPCManager() { }
    static NPCManager() { }
    public static NPCManager Instance
    {
        get
        {
            return instance;
        }
    }
    public List<NPC> NPCs = new List<NPC>();
    public Dictionary<string, Action> CustomDialogFunctions = new Dictionary<string, Action>();
    public async Task LoadNPCs(HttpClient Http)
    {
        List<string> npcs = new List<string>();
        npcs.AddRange(await Http.GetFromJsonAsync<string[]>("data/NPCs.json"));
        foreach(string s in npcs)
        {
            NPCs.Add(await Http.GetFromJsonAsync<NPC>("data/NPCs/" + s + ".json"));
        }
        CustomDialogFunctions.Add("GetPlaytime", new Action(() => GetPlaytime()));
        foreach(NPC npc in NPCs)
        {
            foreach(Dialog d in npc.Dialogs)
            {
                if(d.ResponseWithParameter == "UnlockArea" + d.Parameter)
                {
                    CustomDialogFunctions.TryAdd("UnlockArea" + d.Parameter, new Action(() => UnlockArea(d.Parameter)));
                }
                else if(d.ResponseWithParameter == "UnlockAndGotoArea" + d.Parameter)
                {
                    CustomDialogFunctions.TryAdd("UnlockAndGotoArea" + d.Parameter, new Action(() => UnlockAndGotoArea(d.Parameter)));
                }
                else if (d.ResponseWithParameter == "GotoCustomArea" + d.Parameter)
                {
                    CustomDialogFunctions.TryAdd("GotoCustomArea" + d.Parameter, new Action(() => GotoCustomArea(d.Parameter)));
                }
                else if (d.ResponseWithParameter == "DieAndGotoArea" + d.Parameter)
                {
                    CustomDialogFunctions.TryAdd("DieAndGotoArea" + d.Parameter, new Action(() => DieAndGotoArea(d.Parameter)));
                }
                else if (d.ResponseWithParameter == "ChangeWorldColor" + d.Parameter)
                {
                    CustomDialogFunctions.TryAdd("ChangeWorldColor" + d.Parameter, new Action(() => ChangeWorldColor(d.Parameter)));
                }
                else if (d.ResponseWithParameter == "GoHunting" + d.Parameter)
                {
                    CustomDialogFunctions.TryAdd("GoHunting" + d.Parameter, new Action(() => GoHunting(d.Parameter)));
                }
            }
        }
        
    }
    public void GoHunting(string huntingInfo)
    {
        HuntingTripInfo info = new HuntingTripInfo();
        Area a = AreaManager.Instance.GetAreaByURL(huntingInfo.Split(':')[0]);
        info.DropTable = ItemManager.Instance.GetMinigameDropTable(huntingInfo.Split(':')[1]).DropTable;
        info.DropTableLocation = huntingInfo.Split(':')[1];
        a.HuntingTripInfo = info;
        int hours = int.Parse(huntingInfo.Split(':')[2]);
        MessageManager.AddMessage("The hunters take you along on a " + hours + " hour hunt.");
        GameState.GoTo("World/SahotaClearing/");
        HuntingManager.StartHuntingTrip(info, hours);
        
    }
    public void ChangeWorldColor(string color)
    {
        GameState.BGColor = color;
        MessageManager.AddMessage("The world changes colors as the man disappears in a puff of smoke.");
    }
    public void GetPlaytime()
    {
        TimeSpan time = TimeSpan.FromMilliseconds(GameState.CurrentTick * GameState.GameSpeed);
        if(time.TotalHours > 1)
        {
            MessageManager.AddMessage("You've been in this world for " + Math.Round(time.TotalHours, 2) + " hours.");
        }
        else if(time.TotalMinutes > 1)
        {
            MessageManager.AddMessage("You've been in this world for " + Math.Round(time.TotalMinutes, 2) + " minutes.");
        }
        else if(time.TotalSeconds > 1)
        {
            MessageManager.AddMessage("You've been in this world for " + time.TotalSeconds + " seconds.");
        }
        else
        {
            MessageManager.AddMessage("You've only been in this world for " + time.TotalMilliseconds + " milliseconds!");
        }
    }
    public void UnlockArea(string name)
    {
        AreaManager.Instance.GetAreaByName(name).IsUnlocked = true;
    }
    public void UnlockAndGotoArea(string name)
    {
        Area a = AreaManager.Instance.GetAreaByName(name);
        a.Unlock();
        GameState.GoTo("World/" + a.AreaURL);
        
    }
    public void DieAndGotoArea(string area)
    {
        MessageManager.AddMessage("The pirates throw you overboard and you drown.");
        Player.Instance.Die();
        MessageManager.AddMessage("You wake up near the docks of Koya Hasa.");
        Area a = AreaManager.Instance.GetAreaByName(area);
        Land l = AreaManager.Instance.GetLandForArea(a);
        if (l != null && l != GameState.CurrentLand)
        {
            GameState.CurrentLand = l;
        }
        a.Unlock();
        GameState.GoTo("World/" + a.AreaURL);
    }
    public void GotoCustomArea(string url)
    {
        GameState.GoTo("World/" + url);
        
    }
    public NPC GetNPCByName(string name)
    {
        NPC npc = NPCs.FirstOrDefault(x => x.Name == name);
        if(npc == null)
        {
            Console.WriteLine("No NPC of name " + name + " was found. Did you add it to NPCs.json?");
            return NPCs[0];
        }
        else
        {
            Console.WriteLine("Found NPC:" + npc.Name);
        }
        return npc;
    }
}

