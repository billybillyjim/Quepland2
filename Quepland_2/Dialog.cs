using System;
using System.Collections.Generic;
using System.Linq;

public class Dialog
{ 
	public string ButtonText { get; set; } = "Unset";
	public string ResponseText { get; set; } = "Unset";

	public string ResponseWithParameter
    {
        get
        {
			return ResponseText + Parameter;
		}
    }
	public string ItemOnTalk { get; set; } = "None";
	public string Quest { get; set; } = "None";
	public string UnlockedFollower { get; set; } = "None";
	public string Parameter { get; set; } = "";
	public bool ConsumeRequiredItems { get; set; }
	public bool CompleteQuest { get; set; } = false;
	public int NewQuestProgressValue { get; set; } = -1;
	public List<Requirement> Requirements { get; set; } = new List<Requirement>();
	private bool HasStartedQuest;
	public int Depth { get; set; }
	public int NewDepthOnTalk { get; set; }

	public bool HasRequirements()
	{
		if(Requirements.Count == 0)
        {
			return true;
        }
		foreach (Requirement r in Requirements)
		{
			if (r.IsMet() == false)
			{
				return false;
			}
		}
		if(UnlockedFollower != "None")
        {
            if (FollowerManager.Instance.GetFollowerByName(UnlockedFollower).IsUnlocked)
            {
				return false;
            }
        }
		return true;
	}
	public void Talk()
    {
		if (NPCManager.Instance.CustomDialogFunctions.TryGetValue(ResponseWithParameter, out Action a))
		{
			a.Invoke();
			if (Quest != "None" && NewQuestProgressValue != -1)
			{
				if (NewQuestProgressValue == 1 && QuestManager.Instance.GetQuestByName(Quest).Progress == 0 && HasStartedQuest == false)
				{
					HasStartedQuest = true;
					MessageManager.AddMessage("You've started the quest " + Quest + ".", "#00ff00");
				}
				QuestManager.Instance.GetQuestByName(Quest).Progress = NewQuestProgressValue;
				if (CompleteQuest)
				{
					QuestManager.Instance.GetQuestByName(Quest).Complete();

				}

			}
			return;
		}
		if (ItemOnTalk != "None")
		{
			if (Player.Instance.Inventory.AddItem(ItemManager.Instance.GetItemByName(ItemOnTalk).Copy()) == false)
			{
				MessageManager.AddMessage("Your inventory is full! Come back after you store something in your bank.", "red");
				return;
			}

		}
        if (ConsumeRequiredItems)
        {
			foreach(Requirement r in Requirements)
            {
				if(r.Item != "None")
                {
					if(Player.Instance.Inventory.GetNumberOfItem(ItemManager.Instance.GetItemByName(r.Item)) < r.ItemAmount)
					{
						if(r.ItemAmount == 1)
                        {
							MessageManager.AddMessage("You need a " + r.Item + ".", "red");
						}
                        else
                        {
							MessageManager.AddMessage("You don't have enough " + r.Item + ".(" + r.ItemAmount + ")", "red");
						}
						
						return;
                    }
                }
            }
			foreach (Requirement r in Requirements)
			{
				if (r.Item != "None")
				{
					Player.Instance.Inventory.RemoveItems(ItemManager.Instance.GetItemByName(r.Item), r.ItemAmount);

				}
			}
		}
		if(UnlockedFollower != "None")
        {
			FollowerManager.Instance.GetFollowerByName(UnlockedFollower).IsUnlocked = true;
        }
		MessageManager.AddMessage(ResponseText);
		if (Quest != "None" && NewQuestProgressValue != -1)
		{
			if (NewQuestProgressValue == 1 && QuestManager.Instance.GetQuestByName(Quest).Progress == 0 && HasStartedQuest == false)
			{
				HasStartedQuest = true;
				MessageManager.AddMessage("You've started the quest " + Quest + ".", "#00ff00");
			}
			QuestManager.Instance.GetQuestByName(Quest).Progress = NewQuestProgressValue;
			if (CompleteQuest)
			{
				QuestManager.Instance.GetQuestByName(Quest).Complete();

			}

		}
	}
}
