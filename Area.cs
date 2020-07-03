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
    public string Image { get; set; } = "NoImage";
    public string Description { get; set; } = "This place is indescribable... Or maybe the dev just forgot to describe it.";
    public bool IsUnlocked { get; set; }
    public bool IsHidden { get; set; }
    public List<string> Actions { get; set; } = new List<string>();
    public List<string> Monsters { get; set; } = new List<string>();
    public List<string> NPCs { get; set; } = new List<string>();
    public List<AreaUnlock> UnlockableAreas { get; set; } = new List<AreaUnlock>();
    public List<Building> Buildings { get; set; } = new List<Building>();
    public HunterTrapSlot TrapSlot { get; set; }
    public HuntingTripInfo HuntingTripInfo { get; set; }
    public string DungeonName { get; set; }
    private Dungeon _dungeon;
    public Dungeon Dungeon
    {
        get
        {
            if(_dungeon == null && DungeonName != null)
            {
                _dungeon = AreaManager.Instance.Dungeons.FirstOrDefault(x => x.Name == DungeonName);
            }
            return _dungeon;
        }
    }
    public List<Shop> Shops { get; set; } = new List<Shop>();

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
        if(UnlockableAreas != null && UnlockableAreas.Count > 0)
        {
            foreach(AreaUnlock unlock in UnlockableAreas)
            {
                if(unlock.HasRequirements() && AreaManager.Instance.GetAreaByURL(unlock.AreaURL).IsUnlocked == false)
                {
                    return true;
                }
            }
        }
        return false;
    }
}

