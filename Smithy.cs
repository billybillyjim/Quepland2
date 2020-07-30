using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public class Smithy
{
	[JsonIgnore]
	public List<GameItem> SmeltableMetals { 
		get { 
			if (smeltable == null) 
			{
				smeltable = new List<GameItem>();
				foreach(string s in SmeltableMetalNames)
                {
					smeltable.Add(ItemManager.Instance.GetItemByName(s));
                }
			}
			return smeltable;
		}
	}
	[JsonIgnore]
	private List<GameItem> smeltable;
	public List<string> SmeltableMetalNames { get; set; }
	public string Location { get; set; }
}
