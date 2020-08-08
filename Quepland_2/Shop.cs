using System;
using System.Collections.Generic;

public class Shop
{
	private List<GameItem> items;
	public List<GameItem> Items
	{
        get
        {
			if(items == null)
            {
				items = new List<GameItem>();
				foreach(string s in ItemNames)
                {
					items.Add(ItemManager.Instance.GetItemByName(s));
                }
            }

			return items;
        }
	}
	public string Name { get; set; }
	public List<string> ItemNames { get; set; }
	public List<Requirement> Requirements { get; set; }
	public double CostMultiplier { get; set; } = 1;
	public GameItem Currency { get
        {
			if(currency == null)
            {
				currency = ItemManager.Instance.GetItemByName(CurrencyName);

			}
			return currency;
        } 
	}
	private GameItem currency;
	public string CurrencyName { get; set; } = "Coins";
	public bool LimitBoughtItemsToSoldItems { get; set; } = true;
	public bool HasItem(GameItem item)
    {
		foreach(GameItem i in Items)
        {
			if(i.Name == item.Name)
            {
				return true;
            }
        }
		return false;
    }
}
