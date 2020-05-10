using System;

public class Requirement
{
	public string Skill { get; set; } = "None";
	public string Item { get; set; } = "None";
	public string Action { get; set; } = "None";
	public string Quest { get; set; } = "None";
	public int QuestProgress { get; set; }
	public int ItemAmount { get; set; }
	public int SkillLevel { get; set; }

	public bool IsMet()
	{
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
		if(Quest != "None" && QuestManager.Instance.GetQuestByName(Quest).Progress != QuestProgress)
        {
			return false;
        }

		return true;
    }
}
