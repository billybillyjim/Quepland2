using System;
using System.Collections.Generic;

[Serializable]
public class GameItem
{
	public string Name { get; set; } = "Unset Name";
	public string Description { get; set; } = "Unset Description";
	public string GatherString { get; set; } = "You get an item";
	public string ExperienceGained { get; set; } = "None";
	public string EnabledActions { get; set; } = "None";
	public string Category { get; set; } = "Unset";

	public string Icon { get; set; } = "Unset";
	public string EquipSlot { get; set; } = "None";

	public bool IsStackable { get; set; }
	public bool IsEquipped { get; set; }
	public bool IsSellable { get; set; } = true;
	public bool Rerender { get; set; } = false;

	public int Value { get; set; } = 1;
	/// <summary>
	/// The number of game ticks it takes on average to acquire one resource.
	/// </summary>
	public int GatherSpeed { get; set; } = 10;
	public int ID { get; set; }
	public double GatherSpeedBonus { get; set; }
	public ArmorInfo ArmorInfo { get; set; }
	public WeaponInfo WeaponInfo { get; set; }
	public SmithingInfo SmithingInfo { get; set; }
	public AlchemyInfo AlchemyInfo { get; set; }
	public FoodInfo FoodInfo { get; set; }
	public TrapInfo TrapInfo { get; set; }
	public TanningInfo TanningInfo { get; set; }
	public List<Requirement> Requirements { get; set; } = new List<Requirement>();

	public bool HasRequirements()
    {
		foreach(Requirement r in Requirements)
        {
			if(r.IsMet() == false)
            {
				return false;
            }
        }
		return true;
    }
	public string GetRequirementTooltip()
    {
        if (HasRequirements())
        {
			return "";
        }
		string req = "";
		foreach (Requirement r in Requirements)
		{
			if (r.IsMet() == false)
			{
				req += r.ToString() + "\n";
			}
		}
		req = req.Substring(0, req.Length - 1);
		return req;
	}
	public string GetSkillForWeaponExp()
    {
		foreach(Requirement req in Requirements)
        {
			if(req.Skill != null)
            {
				return req.Skill;
            }
        }
		Console.WriteLine("Weapon " + Name + " has no requirement for any skill, so no experience can be given.");
		return "";
    }

	public override string ToString()
	{
		return Name;
	}
	public GameItem Copy()
    {
		GameItem copy = new GameItem();
		copy.Name = Name;
		copy.Description  = Description;
		copy.GatherString = GatherString;
		copy.ExperienceGained = ExperienceGained;
		copy.EnabledActions = EnabledActions;
		copy.Category = Category;

		copy.Icon = Icon;
		copy.EquipSlot = EquipSlot;

		copy.IsStackable = IsStackable;
		copy.IsSellable = IsSellable;

		copy.Value = Value;

		copy.GatherSpeed = GatherSpeed;
		copy.ID = ID;
		copy.GatherSpeedBonus = GatherSpeedBonus;
		copy.ArmorInfo = ArmorInfo;
		copy.WeaponInfo = WeaponInfo;
		copy.SmithingInfo = SmithingInfo;
		copy.AlchemyInfo = AlchemyInfo;
		copy.FoodInfo = FoodInfo;
		copy.TrapInfo = TrapInfo;
		copy.TanningInfo = TanningInfo;
		copy.Requirements = Requirements;
		return copy;
    }
}
