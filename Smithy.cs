using System;
using System.Collections.Generic;

public class Smithy
{
	public List<GameItem> SmithableMetals { 
		get { 
			if (smithable == null) 
			{
				smithable = new List<GameItem>();
				foreach(string s in SmithableMetalNames)
                {
					smithable.Add(ItemManager.Instance.GetItemByName(s));
                }
			}
			return smithable;
		} 
	}
	private List<GameItem> smithable;
	public List<string> SmithableMetalNames { get; set; }
	public string Location { get; set; }
}
