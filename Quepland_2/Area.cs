using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


public class Area
{
    public string Name { get; set; } = "Unset";
    private string _areaURL;
    public string AreaURL { get
        {
            if (_areaURL != null)
            {
                return _areaURL;
            }
            return Name;
        }
        set
        {
            _areaURL = value;
        }
    }
    public int ID { get; set; }
    [JsonIgnore]
    public string Image { get; set; } = "NoImage";
    [JsonIgnore]
    public string Description { get; set; } = "This place is indescribable... Or maybe the dev just forgot to describe it.";
    public bool IsUnlocked { get; set; }
    [JsonIgnore]
    public bool IsHidden { get; set; }

    public string DojoURL { get; set; }
    [JsonIgnore]
    private Dojo dojo;
    [JsonIgnore]
    public Dojo Dojo { get
        {
            if (dojo == null)
            {
                dojo = AreaManager.Instance.GetDojoByURL(DojoURL);
            }
            return dojo;
        }
    }
    [JsonIgnore]
    public List<string> Actions { get; set; } = new List<string>();
    public List<string> ConversionActions { get; set; } = new List<string>();
    [JsonIgnore]
    public List<string> Monsters { get; set; } = new List<string>();
    [JsonIgnore]
    public List<string> NPCs { get; set; } = new List<string>();
    [JsonIgnore]
    public List<AreaUnlock> UnlockableAreas { get; set; } = new List<AreaUnlock>();
    [JsonIgnore]
    public List<Building> Buildings { get; set; } = new List<Building>();
    public HunterTrapSlot TrapSlot { get; set; }
    public HuntingTripInfo HuntingTripInfo { get; set; }
    public string DungeonName { get; set; }
    private Dungeon _dungeon;
    [JsonIgnore]
    public Dungeon Dungeon
    {
        get
        {
            if (_dungeon == null && DungeonName != null)
            {
                _dungeon = AreaManager.Instance.Dungeons.FirstOrDefault(x => x.Name == DungeonName);
            }
            return _dungeon;
        }
    }
    [JsonIgnore]
    public List<Shop> Shops { get; set; } = new List<Shop>();
    [JsonIgnore]
    public List<Requirement> Requirements { get; set; } = new List<Requirement>();
    public List<AFKAction> AFKActions { get; set; } = new List<AFKAction>();

    public bool HasRequirements()
    {
        foreach (Requirement r in Requirements)
        {
            if (r.IsMet() == false)
            {
                return false;
            }
        }
        return true;
    }
    public bool TrapSlotIsReady()
    {
        if(TrapSlot == null)
        {
            return false;
        }
        if(TrapSlot.State != "Set")
        {
            return false;
        }
        return TrapSlot.HarvestTime.CompareTo(DateTime.UtcNow) <= 0;
    }
    public string GetRequirementTooltip()
    {
        if (HasRequirements())
        {
            return "";
        }
        string req = "";

        bool hasEquipInfo = false;

        if (hasEquipInfo == false)
        {
            foreach (Requirement r in Requirements)
            {
                if (r.IsMet() == false)
                {
                    req += r.ToString().Replace("tools", "means") + "\n";
                }
            }
        }
        req = req.Substring(0, req.Length - 1);
        return req;
    }
    public Building GetBuildingByURL(string url)
    {
        return Buildings.FirstOrDefault(x => x.URL == url);
    }
    public void Unlock()
    {
        IsUnlocked = true;
        AreaManager.Instance.GetRegionForArea(this).IsUnlocked = true;
    }
    public bool HasUnlockableAreas()
    {
        if (UnlockableAreas != null && UnlockableAreas.Count > 0)
        {
            foreach (AreaUnlock unlock in UnlockableAreas)
            {
                if (unlock == null)
                {
                    Console.WriteLine("Area " + Name + " has a null value.");
                }
                if (unlock.HasRequirements() && AreaManager.Instance.GetAreaByURL(unlock.AreaURL).IsUnlocked == false)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public AreaSaveData GetSaveData()
    {
        return new AreaSaveData { IsUnlocked = IsUnlocked,
            TrapHarvestTime = TrapSlot?.HarvestTime ?? DateTime.MinValue,
            TrapState = TrapSlot?.State ?? "Unset",
            TripIsActive = HuntingTripInfo?.IsActive ?? false,
            TripReturnTime = HuntingTripInfo?.ReturnTime ?? DateTime.MinValue,
            TripStartTime = HuntingTripInfo?.StartTime ?? DateTime.MinValue,
            TrapSize = TrapSlot?.Size ?? 0,
            dtLocation = HuntingTripInfo?.DropTableLocation ?? "",
            ID = ID };
    }
    public void LoadSaveData(AreaSaveData data)
    {
        IsUnlocked = data.IsUnlocked;
        if(TrapSlot != null)
        {
            TrapSlot.Size = data.TrapSize;
            TrapSlot.State = data.TrapState;
            TrapSlot.HarvestTime = data.TrapHarvestTime;
        }
        if(HuntingTripInfo != null)
        {
            try
            {

                HuntingTripInfo.LoadSaveData(data.TripIsActive, data.TripReturnTime, data.TripStartTime, data.HuntingBoost, "");
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
        else if (data.TripIsActive)
        {
            if(HuntingTripInfo != null)
            {
                HuntingTripInfo.LoadSaveData(data.TripIsActive, data.TripReturnTime, data.TripStartTime, data.HuntingBoost, data.dtLocation);

            }
            else
            {
                HuntingTripInfo = new HuntingTripInfo();
                HuntingTripInfo.LoadSaveData(data.TripIsActive, data.TripReturnTime, data.TripStartTime, data.HuntingBoost, data.dtLocation);
            }
        }
        
    }
}

