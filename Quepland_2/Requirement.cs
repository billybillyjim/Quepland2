using System;

public class Requirement
{
	public string Skill { get; set; } = "None";
	public string Item { get; set; } = "None";
	public string Action { get; set; } = "None";
	public string Location { get; set; } = "None";
	public string Quest { get; set; } = "None";
	public string AreaUnlocked { get; set; } = "None";
	public string LockedFollower { get; set; } = "None";
	public bool RequireAreaLocked { get; set; } = false;
	/// <summary>
	/// The inclusive minimum step the quest must be at to fulfill the requirement.
	/// </summary>
	public int MinimumQuestProgress { get; set; }
	/// <summary>
	/// The inclusive maximum step the quest must be at to fulfill the requirement. Max value if unset.
	/// </summary>
	public int MaximumQuestProgress { get; set; } = int.MaxValue;
	public int ItemAmount { get; set; }
	public int SkillLevel { get; set; }
	/// <summary>
	/// The maximum level a skill can be at to fulfill the requirement. Inclusive.
	/// </summary>
	public int SkillLimit { get; set; } = int.MaxValue - 10;

	public bool IsMet()
	{
        try
        {
			if (Location != "None" && GameState.Location != Location)
			{
				return false;
			}
			if (Skill != "None")
			{
				if (Player.Instance.HasSkillRequirement(Skill, SkillLevel) == false)
                {
					return false;
				}
				else if (Player.Instance.HasSkillRequirement(Skill, SkillLimit + 1))
                {
					Console.WriteLine("Player has too high of skill:" + SkillLimit);
					return false;
                }
			}			
			if (Action != "None" && Player.Instance.HasToolRequirement(Action) == false)
			{
				return false;
			}
			if (Item != "None" && Player.Instance.Inventory.GetNumberOfItem(ItemManager.Instance.GetItemByName(Item)) < ItemAmount)
			{			
				return false;
			}
			if (LockedFollower != "None" && FollowerManager.Instance.GetFollowerByName(LockedFollower).IsUnlocked)
			{
				return false;
			}
			if (AreaUnlocked != "None")
			{
				if (RequireAreaLocked)
				{
					if (AreaManager.Instance.GetAreaByName(AreaUnlocked).IsUnlocked)
					{
						return false;
					}
				}
				else
				{
					if (AreaManager.Instance.GetAreaByName(AreaUnlocked).IsUnlocked == false)
					{
						return false;
					}
				}
			}
			if (Quest != "None")
			{
				int progress = QuestManager.Instance.GetQuestByName(Quest).Progress;
				if (progress < MinimumQuestProgress || progress > MaximumQuestProgress)
				{
					return false;
				}
			}

			return true;
		}
        catch(Exception e)
        {
			Console.WriteLine(e.Message);
			Console.WriteLine(e.StackTrace);
			return false;
        }
		
    }

    public override string ToString()
    {
		string req = "";
		if (Location != "None" && GameState.Location != Location)
		{
			req += "You must be at " + Location;
		}
		if (Skill != "None" && Player.Instance.HasSkillRequirement(Skill, SkillLevel) == false)
		{
			req += "You need " + SkillLevel + " " + Skill;
		}
		else if (Player.Instance.HasSkillRequirement(Skill, SkillLimit + 1))
		{
			req += ("Player has too high of skill:" + SkillLimit);
		}
		if (Action != "None" && Player.Instance.HasToolRequirement(Action) == false)
		{
			req += "You don't have any tools for " + Action;
		}
		if (Item != "None" && Player.Instance.Inventory.GetNumberOfItem(ItemManager.Instance.GetItemByName(Item)) < ItemAmount)
		{
			req += "You need " + ItemAmount + " " + Item;
		}
		if (Quest != "None")
		{
			int progress = QuestManager.Instance.GetQuestByName(Quest).Progress;
			if (progress < MinimumQuestProgress || progress > MaximumQuestProgress)
			{
				req += "You need to progress further in the quest " + Quest;
			}
		}
		return req;
    }
	public string AllRequirementsString()
    {
		string req = "";
		if (Location != "None")
		{
			req += "You must be at " + Location;
		}
		if (Skill != "None")
		{
			req += "You need " + SkillLevel + " " + Skill;
		}
		if (Action != "None")
		{
			req += "You need a tool for " + Action;
		}
		if (Item != "None")
		{
			req += "You need " + ItemAmount + " " + Item;
		}
		if (Quest != "None")
		{
			int progress = QuestManager.Instance.GetQuestByName(Quest).Progress;
			req += "You need progress in the quest " + Quest;
			
		}
		return req;
	}
}
