using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;


public class AreaManager
{
    private static readonly AreaManager instance = new AreaManager();
    private AreaManager() { }
    static AreaManager() { }
    public static AreaManager Instance
    {
        get
        {
            return instance;
        }
    }
    public List<Area> Areas = new List<Area>();
    public List<Dungeon> Dungeons = new List<Dungeon>();
    public async Task LoadAreas(HttpClient Http)
    {
        Areas.AddRange(await Http.GetJsonAsync<Area[]>("data/Areas/MountQueple.json"));
        Dungeons.AddRange(await Http.GetJsonAsync<Dungeon[]>("data/Dungeons/QueplandDungeons.json"));
    }
    public Area GetAreaByName(string name)
    {
        return Areas.FirstOrDefault(x => x.Name == name);
    }
    public Area GetAreaByURL(string url)
    {
        return Areas.FirstOrDefault(x => x.AreaURL == url);
    }
}

