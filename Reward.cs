using System;

public class Reward
{
	public string Skill { get; set; } = "None";
	public string Item { get; set; } = "None";
	public int ItemAmount { get; set; }
	public long ExperienceGain { get; set; }

    public void Award()
    {
        if (Skill != "None")
        {
            Player.Instance.GainExperience(Skill, ExperienceGain);
        }
        if (Item != "None")
        {
            Player.Instance.Inventory.AddMultipleOfItem(ItemManager.Instance.GetItemByName(Item), ItemAmount);
        }
    }
    public override string ToString()
    {
        string text = "";
        if(Skill != "None")
        {
            text += ExperienceGain + " " + Skill + " Experience";
        }
        if(Item != "None")
        {
            text += ItemAmount + " " + Item;
        }
        return text;
    }
}
