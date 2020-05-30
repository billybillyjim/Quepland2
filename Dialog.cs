using System;
using System.Collections.Generic;

public class Dialog
{ 
	public string ButtonText { get; set; } = "Unset";
	public string ResponseText { get; set; } = "Unset";
	public string ItemOnTalk { get; set; } = "None";
	public string Quest { get; set; } = "None";
	public bool ConsumeRequiredItems { get; set; }
	public int NewQuestProgressValue { get; set; } = -1;
	public List<Requirement> Requirements { get; set; } = new List<Requirement>();

	public bool HasRequirements()
	{
		foreach (Requirement r in Requirements)
		{
			if (r.IsMet() == false)
			{
				return false;
			}
		}

		return true;
	}
	public void Talk()
    {
		if(Quest != "None" && NewQuestProgressValue != -1)
        {
			QuestManager.Instance.GetQuestByName(Quest).Progress = NewQuestProgressValue;

		}
		if(ItemOnTalk != "None")
        {
			Player.Instance.Inventory.AddItem(ItemManager.Instance.GetItemByName(ItemOnTalk));
			
        }
        if (ConsumeRequiredItems)
        {
			foreach(Requirement r in Requirements)
            {
				if(r.Item != "None")
                {
					Player.Instance.Inventory.RemoveItems(ItemManager.Instance.GetItemByName(r.Item), r.ItemAmount);
                }
            }
        }
		MessageManager.AddMessage(ResponseText);
	}
}
