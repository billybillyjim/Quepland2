using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;


public class FollowerManager
{
    private static readonly FollowerManager instance = new FollowerManager();
    private FollowerManager() { }
    static FollowerManager() { }
    public static FollowerManager Instance
    {
        get
        {
            return instance;
        }
    }
    public List<Follower> Followers = new List<Follower>();
    public async Task LoadFollowers(HttpClient Http)
    {
        Followers.AddRange(await Http.GetJsonAsync<Follower[]>("data/Followers.json"));
    }
    public List<Follower> GetUnlockedFollowers()
    {
        return Followers.Where(x => x.IsUnlocked).ToList();
    }
    public Follower GetFollowerByName(string name)
    {
        return Followers.FirstOrDefault(x => x.Name == name);
    }
    public string GetSaveData()
    {
        string data = "";
        foreach(Follower f in Followers)
        {
            data += f.Name + ":" + f.IsUnlocked + ",";
        }
        return data;
    }
    public void LoadSaveData(string data)
    {
        string[] lines = data.Split(',');
        foreach(string line in lines)
        {
            if(line.Length < 2)
            {
                continue;
            }
            string name = line.Split(':')[0];
            bool unlock = bool.Parse(line.Split(':')[1]);
            Followers.Find(x => x.Name == name).IsUnlocked = unlock;
        }
    }
}

