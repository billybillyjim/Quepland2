using System;
using System.Collections.Generic;

public class TomeData
{
	public string Category { get; set; } = "Unset";
	public List<string> ItemNames { get; set; } = new List<string>();
	public TomeData(string category)
    {
		Category = category;
    }
	public void AddItem(string itemName)
    {
		if(ItemNames.Contains(itemName) == false)
        {
            ItemNames.Add(itemName);
        }
    }
}
