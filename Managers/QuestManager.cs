using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;


public class QuestManager
{
    private static readonly QuestManager instance = new QuestManager();
    private QuestManager() { }
    static QuestManager() { }
    public static QuestManager Instance
    {
        get
        {
            return instance;
        }
    }
    public List<Quest> Quests = new List<Quest>();

    public async Task LoadQuests(HttpClient Http)
    {
        Quests.AddRange(await Http.GetJsonAsync<Quest[]>("data/Quests.json"));
    }

    public Quest GetQuestByName(string name)
    {
        return Quests.FirstOrDefault(x => x.Name == name);
    }
}

