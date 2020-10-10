using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
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
        /*
        //For checking for missing npcs
        Console.WriteLine("Total NPCs:" + npcs.Count);
        List<string> npcCheck = new List<string>();
        npcCheck.AddRange(npcs);
        foreach(Area a in AreaManager.Instance.Areas)
        {
            foreach(Building b in a.Buildings)
            {
                foreach(string s in b.NPCs)
                {
                    npcCheck.Remove(s);
                }
            }
            foreach (string s in a.NPCs)
            {
                npcCheck.Remove(s);
            }
        }
        Console.WriteLine("Total Missing NPCs:" + npcCheck.Count);
        foreach(string n in npcCheck)
        {
            Console.WriteLine("Missing:" + n);
        }
        */
        CustomDialogFunctions.Add("GetPlaytime", new Action(() => GetPlaytime()));
        foreach(NPC npc in NPCs)
        {
            foreach(Dialog d in npc.Dialogs)
            {
                if (d.ResponseWithParameter == "UnlockArea" + d.Parameter)
                {
                    CustomDialogFunctions.TryAdd("UnlockArea" + d.Parameter, new Action(() => UnlockArea(d.Parameter)));
                }
                else if (d.ResponseWithParameter == "UnlockAndGotoArea" + d.Parameter)
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
                else if(d.ResponseWithParameter == "FightOpponents" + d.Parameter)
                {
                    CustomDialogFunctions.TryAdd("FightOpponents" + d.Parameter, new Action(() => FightEnemies(d.Parameter)));
                }
                else if (d.ResponseWithParameter == "AssignTask" + d.Parameter)
                {
                    CustomDialogFunctions.TryAdd("AssignTask" + d.Parameter, new Action(() => GetNewArtisanTask(d.Parameter)));
                }
                else if (d.ResponseWithParameter == "CancelTask" + d.Parameter)
                {
                    CustomDialogFunctions.TryAdd("CancelTask" + d.Parameter, new Action(() => CancelTask(d.Parameter)));
                }
                else if (d.ResponseWithParameter == "GetTaskCost" + d.Parameter)
                {
                    CustomDialogFunctions.TryAdd("GetTaskCost" + d.Parameter, new Action(() => GetTaskCost(d.Parameter)));
                }
                else if (d.ResponseWithParameter == "IdentifyGems" + d.Parameter)
                {
                    CustomDialogFunctions.TryAdd("IdentifyGems" + d.Parameter, new Action(() => IdentifyGems()));
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
        AreaManager.Instance.GetAreaByName(name).Unlock();
    }
    public void UnlockAndGotoArea(string name)
    {
        Area a = AreaManager.Instance.GetAreaByName(name);
        Land l = AreaManager.Instance.GetLandForArea(a);
        if (l != null && l != GameState.CurrentLand)
        {
            GameState.CurrentLand = l;
        }
        a.Unlock();
        GameState.GoTo("World/" + a.AreaURL);
        
    }
    public void GetNewArtisanTask(string skills)
    {
        if(GameState.CurrentArtisanTask == null)
        {
            string[] s = skills.Split(',');
            string skill = s[GameState.Random.Next(0, s.Length)];
            GameState.CurrentArtisanTask = ItemManager.Instance.GetNewArtisanTask(skill);

            MessageManager.AddMessage("You've been asked to make " + GameState.CurrentArtisanTask.AmountRequired + " " + GameState.CurrentArtisanTask.Item.GetPlural());
            SaveManager.SaveGame();
        }
        else
        {
            MessageManager.AddMessage("You already have a task from the guild. " + GameState.CurrentArtisanTask.ToString());
        }
    }
    public void CancelTask(string s)
    {
        if(GameState.CurrentArtisanTask == null)
        {
            MessageManager.AddMessage("You don't have an artisan task right now.");
            return;
        }
        if(s == "Free")
        {
            if(Player.Instance.Inventory.GetNumberOfItem(ItemManager.Instance.GetItemByName("Coins")) >= GameState.CurrentArtisanTask.GetGoldCost() * 10)
            {
                Player.Instance.Inventory.RemoveItems(ItemManager.Instance.GetItemByName("Coins"), GameState.CurrentArtisanTask.GetGoldCost() * 10);
                GameState.CancelTask();
                MessageManager.AddMessage("Nasitra takes your task from you. You can get another from any guild member.");
            }
            else
            {
                MessageManager.AddMessage("You don't have enough gold.");
            }

        }
        else if (Player.Instance.Inventory.RemoveItems(ItemManager.Instance.GetItemByName("Guild Exception Notice"), 1) == 1)
        {
            GameState.CancelTask();
            MessageManager.AddMessage("Your task has been officially cancelled and the guild has redirected the request to another member. Speak to any guild member to recieve a new artisan task.");

        }

    }
    public void GetTaskCost(string s)
    {
        if(GameState.CurrentArtisanTask == null)
        {
            MessageManager.AddMessage("Well, you don't have an artisan task right now... so I can't do it for you.");
        }
        else
        {
            MessageManager.AddMessage("With a task of " + GameState.CurrentArtisanTask.ToString() + ", it'll be " + (GameState.CurrentArtisanTask.GetGoldCost() * 10) + " coins.");

        }
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
    public void FightEnemies(string opponentNames)
    {
        string[] opponents = opponentNames.Split(',');
        foreach(string o in opponents)
        {
            BattleManager.Instance.CurrentOpponents.Add(BattleManager.Instance.GetMonsterByName(o));
            
        }
        BattleManager.Instance.ReturnLocation = "World/Ricechild/";
        BattleManager.Instance.StartBattle();
        GameState.GoTo("World/Battle");
    }
    public void IdentifyGems()
    {
        List<KeyValuePair<GameItem, int>> items = Player.Instance.Inventory.GetItems();
        List<KeyValuePair<GameItem, int>> itemsToRemove = new List<KeyValuePair<GameItem, int>>();
        int amt = 0;
        int coinsToPay = 0;
        foreach (KeyValuePair<GameItem, int> pair in items)
        {
            if(pair.Key.Name == "Unidentified Gem")
            {
                if (Player.Instance.Inventory.GetNumberOfItem(ItemManager.Instance.GetItemByName("Coins")) - coinsToPay >= 300)
                {
                    coinsToPay += 300;    
                    itemsToRemove.Add(new KeyValuePair<GameItem, int>(pair.Key, 1));

                }
            }

        }
        Player.Instance.Inventory.RemoveItems(ItemManager.Instance.GetItemByName("Coins"), coinsToPay);
        foreach (KeyValuePair<GameItem, int> pair in itemsToRemove)
        {
            if (Player.Instance.Inventory.RemoveItems(pair.Key, 1) == 1)
            {
                GameItem i = ItemManager.Instance.GetCopyOfItem(pair.Key.Parameter);
                Player.Instance.Inventory.AddItem(i);
                amt++;

            }
        }
        if(amt == 0)
        {
            MessageManager.AddMessage("You'll need both money and gems to have me identify them...");
        }
        else
        {
            MessageManager.AddMessage("Tochtei correctly identifies your gems.");
        }
        
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

