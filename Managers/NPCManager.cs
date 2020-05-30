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
        List<string> npcs = new List<string>();
        npcs.AddRange(await Http.GetJsonAsync<string[]>("data/NPCs.json"));
        foreach(string s in npcs)
        {
            NPCs.Add(await Http.GetJsonAsync<NPC>("data/NPCs/" + s + ".json"));
        }
        foreach(NPC npc in NPCs)
        {
            Console.WriteLine(npc.Name);
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

