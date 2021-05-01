using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
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
    public List<Building> Buildings = new List<Building>();
    public List<Region> Regions = new List<Region>();
    public List<Land> Lands = new List<Land>();
    public List<Dungeon> Dungeons = new List<Dungeon>();
    public List<Smithy> Smithies = new List<Smithy>();
    public List<Dojo> Dojos = new List<Dojo>();
    public List<AFKAction> AFKActions = new List<AFKAction>();
    public static bool LoadedHuntingInfo = false;
    public async Task LoadAreas(HttpClient Http)
    {
        Regions.AddRange(await Http.GetFromJsonAsync<Region[]>("data/Regions.json"));
        foreach(Region region in Regions)
        {
            Areas.AddRange(await Http.GetFromJsonAsync<Area[]>("data/Areas/" + region.Name.RemoveWhitespace() + ".json"));
            GameState.GameLoadProgress++;
        }
        List<string> UsedNPCs = new List<string>();
        foreach(Area a in Areas)
        {
            foreach(Building b in a.Buildings)
            {
                Buildings.Add(b);
                if(b.AFKActions.Count > 0)
                {
                    AFKActions.AddRange(b.AFKActions);
                }
            }
            if(a.AFKActions.Count > 0)
            {
                AFKActions.AddRange(a.AFKActions);
            }
        }
        
        var query = Areas.GroupBy(x => x.ID).Where(g => g.Count() > 1).Select(y => y.Key).ToList();
        foreach(int val in query)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Areas contain duplicate ID:" + val);
            Console.ForegroundColor = ConsoleColor.Black;
        }
        
        Lands.AddRange(await Http.GetFromJsonAsync<Land[]>("data/Lands.json"));
        GameState.GameLoadProgress++;
        Dungeons.AddRange(await Http.GetFromJsonAsync<Dungeon[]>("data/Dungeons/QueplandDungeons.json"));
        GameState.GameLoadProgress++;
        Smithies.AddRange(await Http.GetFromJsonAsync<Smithy[]>("data/Smithies.json"));
        GameState.GameLoadProgress++;
        Dojos.AddRange(await Http.GetFromJsonAsync<Dojo[]>("data/Dojos.json"));
        GameState.GameLoadProgress++;
        //Console.WriteLine("Quepland consists of " + Areas.Count + " areas, " + Regions.Count + " regions, " + Lands.Count + " lands, with " + Dungeons.Count + " dungeons.");
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
            Dungeon d = Dungeons.FirstOrDefault(x => x.URL == url);
            if(d != null)
            {
                return Areas.FirstOrDefault(x => x.DungeonName == d.Name);
            }
            Console.WriteLine("No area " + url + " found. Have you addded it to Regions.json? Otherwise the URL is incorrect.");
        }
        return a;
    }
    public bool AreaURLIsValid(string url)
    {
        Area a = Areas.FirstOrDefault(x => x.AreaURL == url);
        if (a == null)
        {
            Dungeon d = Dungeons.FirstOrDefault(x => x.URL == url);
            if (d == null)
            {
                return false;
            }
        }
        return true;
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
    public List<AreaSaveData> GetAreaSave()
    {
        List<AreaSaveData> data = new List<AreaSaveData>();
        foreach (Area a in Areas)
        {
            data.Add(a.GetSaveData());
        }
        return data;
    }
    public void LoadAreaSave(List<AreaSaveData> data)
    {
        foreach(AreaSaveData d in data)
        {
            if (d.TripIsActive)
            {
                LoadedHuntingInfo = true;
            }
            Areas.Find(x => x.ID == d.ID).LoadSaveData(d);
        }
        
    }
    public List<RegionSaveData> GetRegionSaveData()
    {
        List<RegionSaveData> data = new List<RegionSaveData>();
        foreach(Region r in Regions)
        {
            data.Add(r.GetSaveData());
        }
        return data;
    }
    public void LoadRegionSave(List<RegionSaveData> data)
    {
        foreach (RegionSaveData d in data)
        {
            Regions.Find(x => x.Name == d.Name).LoadSaveData(d);
        }
    }
    public List<TanningSaveData> GetTanningSaveData()
    {
        List<TanningSaveData> data = new List<TanningSaveData>();

        foreach (Building b in Buildings)
        {
            if (b.TanningSlots.Count > 0)
            {
                foreach (TanningSlot slot in b.TanningSlots)
                {
                    TanningSaveData d = slot.GetSaveData();
                    d.BuildingName = b.Name;
                    data.Add(d);
                }
            }
        }
        

        return data;
    }
    public void LoadTanningSave(List<TanningSaveData> data)
    {

        foreach (Building b in Buildings)
        {
            b.LoadedTanningSlotsIterator = 0;
        }
        

        foreach(TanningSaveData d in data)
        {
            try
            {
                foreach(Area a in Areas)
                {
                    foreach(Building b in a.Buildings)
                    {
                        if(d.BuildingName == b.Name)
                        {
                            b.LoadTanningData(d);
                        }
                    }
                }
            }
            catch
            {
                if(d != null)
                {
                    Console.WriteLine("Failed to load tanning data for building." + d.BuildingName);

                }
                else
                {
                    Console.WriteLine("Data was null");
                }
            }
        }
    }
    public List<DojoSaveData> GetDojoSaveData()
    {
        List<DojoSaveData> data = new List<DojoSaveData>();
        foreach(Dojo d in Dojos)
        {
            data.Add(d.GetSaveData());
        }
        return data;
    }
    public void LoadDojoSaveData(List<DojoSaveData> data)
    {
        foreach(DojoSaveData d in data)
        {
            Dojos.Find(x => x.Name == d.Name).LastWinTime = d.LastWin;
        }
    }
    public List<DungeonSaveData> GetDungeonSaveData()
    {
        List<DungeonSaveData> data = new List<DungeonSaveData>();
        foreach(Dungeon d in Dungeons)
        {
            data.Add(d.GetSaveData());
        }
        return data;
    }
    public void LoadDungeonSaveData(List<DungeonSaveData> data)
    {
        foreach(DungeonSaveData save in data)
        {
            Dungeons.FirstOrDefault(x => x.Name == save.Name).LoadSaveData(save);
        }
    }
    public Area GetAreaByAvailableResource(string itemName)
    {
        foreach(Area a in Areas)
        {
            if(a.IsUnlocked && a.Actions.FirstOrDefault(x => x.Contains(itemName)) != null)
            {
                return a;
            }
        }
        return null;
    }
    public AFKAction GetAFKActionByUniqueID(string id)
    {
        AFKAction action = AFKActions.FirstOrDefault(x => x.UniqueID == id);
        if(action == null)
        {
            Console.WriteLine("Action with unique id:" + id + " was not found.");
        }
        return action;
    }
}

