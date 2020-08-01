using System;

public class TanningSlot
{
	public DateTime FinishTime;
	public int Amount;
	public GameItem TannedItem;
	public bool IsUnlocked;

	public void LoadData(TanningSaveData data)
    {
		IsUnlocked = data.IsUnlocked;
		if(data.TannedItemName != "None")
        {
			TannedItem = ItemManager.Instance.GetCopyOfItem(data.TannedItemName);
			FinishTime = data.FinishTime;
			Amount = data.Amount;
		}

    }
	public TanningSaveData GetSaveData()
	{
		return new TanningSaveData
		{
			IsUnlocked = IsUnlocked,
			Amount = Amount,
			TannedItemName = TannedItem?.Name ?? "None",
			FinishTime = FinishTime
		};
    }
}
