using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
        npcs.AddRange(await Http.GetJsonAsync<string[]>("data/NPCs.json"));
        foreach(string s in npcs)
        {
            NPCs.Add(await Http.GetJsonAsync<NPC>("data/NPCs/" + s + ".json"));
        }
        CustomDialogFunctions.Add("GetPlaytime", new Action(() => GetPlaytime()));
        foreach(NPC npc in NPCs)
        {
            foreach(Dialog d in npc.Dialogs)
            {
                if(d.ResponseText == "UnlockArea")
                {
                    CustomDialogFunctions.Add("UnlockArea", new Action(() => UnlockArea(d.Parameter)));
                }
                else if(d.ResponseText == "UnlockAndGotoArea")
                {
                    CustomDialogFunctions.Add("UnlockAndGotoArea", new Action(() => UnlockAndGotoArea(d.Parameter)));
                }
                else if (d.ResponseText == "GotoCustomArea")
                {
                    CustomDialogFunctions.Add("GotoCustomArea", new Action(() => GotoCustomArea(d.Parameter)));
                }
            }
        }
        
    }
    public void GetPlaytime()
    {
        TimeSpan time = TimeSpan.FromMilliseconds(GameState.CurrentTick * GameState.GameSpeed);
        if(time.TotalHours > 1)
        {
            MessageManager.AddMessage("You've been in this world for " + time.TotalHours + " hours.");
        }
        else if(time.TotalMinutes > 1)
        {
            MessageManager.AddMessage("You've been in this world for " + time.TotalMinutes + " minutes.");
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
        return npc;
    }
}

