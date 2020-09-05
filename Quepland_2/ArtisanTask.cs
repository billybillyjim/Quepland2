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
	public int PointsToEarn { get; set; }

	public ArtisanTask(string itemName, int required)
    {
		ItemName = itemName;
		AmountRequired = required;
		PointsToEarn = Math.Max(2, (AmountRequired / 10) * (Item.Value / 100));
    }
}
