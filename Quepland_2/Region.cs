using System;
using System.Collections.Generic;

public class Region
{
	public string Name { get; set; }
	public List<Area> Areas { get 
		{ 
			if (_areas != null) { return _areas; }
            else
            {
				_areas = new List<Area>();
				foreach(string area in AreaNames)
                {
					Area a = AreaManager.Instance.GetAreaByName(area);
					if(a == null)
                    {
						Console.WriteLine("Failed to load Area:" + area + " for Region:" + Name);
                    }
					_areas.Add(a);
                }
				return _areas;
            }
		}
	}
	private List<Area> _areas { get; set; }
	public List<string> AreaNames { get; set; }
	public bool IsUnlocked { get; set; }

	public void LoadSaveData(RegionSaveData data)
    {
		IsUnlocked = data.IsUnlocked;
    }
	public RegionSaveData GetSaveData()
    {
		return new RegionSaveData { IsUnlocked = IsUnlocked, Name = Name };
    }
}
