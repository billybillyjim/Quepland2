using System;

public class ArtisanTask
{
	private GameItem item;
	public GameItem Item { 
		get
		{
			if(item == null)
            {
				item = ItemManager.Instance.GetItemByName(ItemName);
            }
			return item;
		}
	}
	public string ItemName { get; set; }
	public int AmountRequired { get; set; }
	public int AmountFulfilled { get; set; }
	public int PointsToEarn { get 
		{
			return Math.Max(2, (AmountRequired / 10) * Math.Max(1, (Item.Value / 100)));
		}
	}


	public ArtisanTask(string itemName, int required)
    {
		ItemName = itemName;
		AmountRequired = required;
    }
	public int GetGoldCost()
	{
		return AmountRequired * Item.Value;
	}
	public override string ToString()
    {
		return "The guild has asked you to make " + AmountRequired + " " + Item.GetPlural() + ".(Progress:" + AmountFulfilled + "/" + AmountRequired + ")";

	}
}
