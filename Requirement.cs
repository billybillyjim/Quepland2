using System;

public class Requirement
{
	public string Skill { get; set; } = "None";
	public string Item { get; set; } = "None";
	public string Action { get; set; } = "None";
	public string Location { get; set; } = "None";
	public string Quest { get; set; } = "None";
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

	public bool IsMet()
	{
		if(Location != "None" && GameState.Location != Location)
        {
			return false;
        }
		if (Skill != "None" && Player.Instance.HasSkillRequirement(Skill, SkillLevel) == false)
        {
			return false;
        }
		if(Action != "None" && Player.Instance.HasToolRequirement(Action) == false)
        {
			return false;
        }
		if(Item != "None" && Player.Instance.Inventory.GetNumberOfItem(ItemManager.Instance.GetItemByName(Item)) < ItemAmount)
        {
			return false;
        }
		if(Quest != "None")
        {
			int progress = QuestManager.Instance.GetQuestByName(Quest).Progress;
			if(progress < MinimumQuestProgress || progress > MaximumQuestProgress)
            {
				return false;
            }
        }

		return true;
    }
}
