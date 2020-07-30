using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
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
    [JsonIgnore]
    public List<Region> Regions = new List<Region>();
    [JsonIgnore]
    public List<Land> Lands = new List<Land>();
    [JsonIgnore]
    public List<Dungeon> Dungeons = new List<Dungeon>();
    [JsonIgnore]
    public List<Smithy> Smithies = new List<Smithy>();
    [JsonIgnore]
    public List<Dojo> Dojos = new List<Dojo>();
    public async Task LoadAreas(HttpClient Http)
    {
        Regions.AddRange(await Http.GetJsonAsync<Region[]>("data/Regions.json"));
        foreach(Region region in Regions)
        {
            Areas.AddRange(await Http.GetJsonAsync<Area[]>("data/Areas/" + region.Name.RemoveWhitespace() + ".json"));
        }
        
        Lands.AddRange(await Http.GetJsonAsync<Land[]>("data/Lands.json"));
        Dungeons.AddRange(await Http.GetJsonAsync<Dungeon[]>("data/Dungeons/QueplandDungeons.json"));
        Smithies.AddRange(await Http.GetJsonAsync<Smithy[]>("data/Smithies.json"));
        Dojos.AddRange(await Http.GetJsonAsync<Dojo[]>("data/Dojos.json"));
        Console.WriteLine("Quepland consists of " + Areas.Count + " areas, " + Regions.Count + " regions, " + Lands.Count + " lands, with " + Dungeons.Count + " dungeons.");
    }

    public Area GetAreaByName(string name)
    {
        return Areas.FirstOrDefault(x => x.Name == name);
    }
    public Area GetAreaByURL(string url)
    {
        Area a = Areas.FirstOrDefault(x => x.AreaURL == url);
        if(a == null)
        {
            Console.WriteLine("No area " + url + " found. Have you addded it to Regions.json? Otherwise the URL is incorrect.");
        }
        return a;
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
    public Land GetLandForArea(Area area)
    {
        Region r = GetRegionForArea(area);
        Land l = Lands.FirstOrDefault(x => x.Regions.Contains(r));
        if(l == null)
        {
            Console.WriteLine("No land with region:" + r.Name + " was found.");
        }
        return l;
    }
    public Dojo GetDojoByURL(string url)
    {
        Dojo d = Dojos.FirstOrDefault(x => x.URL == url);
        if(d != null)
        {
            return d;
        }
        Console.WriteLine("Failed to find dojo with URL:" + url);
        return null;
    }
}

