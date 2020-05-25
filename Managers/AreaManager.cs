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
    public List<Region> Regions = new List<Region>();
    public List<Land> Lands = new List<Land>();
    public List<Dungeon> Dungeons = new List<Dungeon>();
    public async Task LoadAreas(HttpClient Http)
    {
        Regions.AddRange(await Http.GetJsonAsync<Region[]>("data/Regions.json"));
        foreach(Region region in Regions)
        {
            Areas.AddRange(await Http.GetJsonAsync<Area[]>("data/Areas/" + region.Name.RemoveWhitespace() + ".json"));
        }
        
        Lands.AddRange(await Http.GetJsonAsync<Land[]>("data/Lands.json"));
        Dungeons.AddRange(await Http.GetJsonAsync<Dungeon[]>("data/Dungeons/QueplandDungeons.json"));
    }

    public Area GetAreaByName(string name)
    {
        return Areas.FirstOrDefault(x => x.Name == name);
    }
    public Area GetAreaByURL(string url)
    {
        //Console.WriteLine(url);
        return Areas.FirstOrDefault(x => x.AreaURL == url);
    }
    public Region GetRegionByName(string name)
    {
        return Regions.FirstOrDefault(x => x.Name == name);
    }
    public Land GetLandByName(string name)
    {
        return Lands.FirstOrDefault(x => x.Name == name);
    }
    public List<Region> GetAvailableRegions(string currentLand)
    {
        return GetLandByName(currentLand).Regions.Where(x => x.IsUnlocked = true).ToList();
    }
    public Region GetRegionForArea(Area area)
    {
        Region r = Regions.FirstOrDefault(x => x.Areas.Contains(area));
        if(r == null)
        {
            Console.WriteLine("No area " + area.Name + " found in any region.");
        }
        return r;
    }
}

