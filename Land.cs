using System;
using System.Collections.Generic;

public class Land
{
	public string Name { get; set; }
	public List<Region> Regions { 
		get 
		{ 
			if(_regions != null)
            {
				return _regions;
            }
            else
            {
				_regions = new List<Region>();
				foreach(string region in RegionNames)
                {
					_regions.Add(AreaManager.Instance.GetRegionByName(region));
                }
				return _regions;
            }
		} 
	}
	private List<Region> _regions { get; set; }
	public List<string> RegionNames { get; set; }
}
