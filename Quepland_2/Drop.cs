using System;

public class Drop
{
	public string ItemName { get; set; } = "Unset";
    private GameItem item;
    public GameItem Item { 
        get
        {
            if(item == null)
            {
                item = ItemManager.Instance.GetCopyOfItem(ItemName, ItemParameter);
            }
            return item;
        } 
    }
    public string ItemParameter { get; set; } = "";
	public int Weight { get; set; } = 1;
	public int Amount { get; set; } = 1;

    public override string ToString()
    {
        return ItemName;
    }
}
