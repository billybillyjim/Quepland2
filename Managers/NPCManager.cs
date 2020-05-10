using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
    public async Task LoadNPCs(HttpClient Http)
    {
        NPCs.AddRange(await Http.GetJsonAsync<NPC[]>("data/NPCs.json"));
    }

    public NPC GetNPCByName(string name)
    {
        NPC npc = NPCs.FirstOrDefault(x => x.Name == name);
        if(npc == null)
        {
            Console.WriteLine("No NPC of name " + name + " was found.");
            return NPCs[0];
        }
        return npc;
    }
}

