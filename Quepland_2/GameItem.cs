using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

[Serializable]
public class GameItem
{
	public string Name { get; set; } = "Unset Name";
	public string Description { get; set; } = "Unset Description";
	public string GatherString { get; set; } = "You get an item";
	public string ExperienceGained { get; set; } = "None";
	public string EnabledActions { get; set; } = "None";
	public string Category { get; set; } = "Unset";
	public string OpenCraftingMenuText { get; set; } = "None";
	public bool IsLocked { get; set; } = false;

	public List<string> Tabs { get; set; } = new List<string>();

	public string Icon { get; set; } = "Unset";
	public string EquipSlot { get; set; } = "None";
	public string PrimaryColor { get; set; } = "white";
	public string SecondaryColor { get; set; } = "lightgray";
	public bool IsTome { get; set; }
	

	public bool PreventArtisanTask { get; set; }
	public int Charges { get; set; } = 0;
	public string Parameter { get; set; } = "";

	public string UniqueID { get
        {
			return Name + Charges + Parameter;
        } 
	}

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
	public double ExperienceGainBonus { get; set; }
	public string ExperienceBonusSkill { get; set; }
	public ArmorInfo ArmorInfo { get; set; }
	public WeaponInfo WeaponInfo { get; set; }
	public AlchemyInfo AlchemyInfo { get; set; }
	public FoodInfo FoodInfo { get; set; }
	public TrapInfo TrapInfo { get; set; }
	public TanningInfo TanningInfo { get; set; }
	public List<Requirement> Requirements { get; set; } = new List<Requirement>();

	public string GetPlural()
    {
		return GameState.Pluralizer.Pluralize(Name);
    }
	public string GetStats()
    {
		string stats = "";
		int dmg = 0;
		int arm = 0;
		int speed = 0;
		if(WeaponInfo != null)
        {
			dmg += WeaponInfo.Damage;
			arm += WeaponInfo.ArmorBonus;
			speed = WeaponInfo.AttackSpeed;
		}
		if(ArmorInfo != null)
        {
			dmg += ArmorInfo.Damage;
			arm += ArmorInfo.ArmorBonus;
		}
		stats = dmg + ",";
		stats += arm + ",";
		if(speed > 0)
        {
			stats += speed;
		}
		return stats;
    }
	public List<string> GetRequiredSkills()
    {
		List<string> reqSkills = new List<string>();
		foreach (Requirement r in Requirements)
		{
			if (r.Skill != "None")
			{
				reqSkills.Add(r.Skill);
			}
		}
		if (ArmorInfo != null)
		{		
			foreach (Requirement r in ArmorInfo.WearRequirements)
			{

				reqSkills.Add(r.Skill);
				
			}
		}
		if (WeaponInfo != null)
		{
			foreach (Requirement r in WeaponInfo.WearRequirements)
			{
				reqSkills.Add(r.Skill);
			}
		}
		return reqSkills;
	}
	public int GetRequirementForSkill(string skillName)
    {
		foreach(Requirement r in Requirements)
        {
			if(r.Skill == skillName)
            {
				return r.SkillLevel;
            }
        }
		if (ArmorInfo != null)
		{
			foreach (Requirement r in ArmorInfo.WearRequirements)
			{

				if (r.Skill == skillName)
				{
					return r.SkillLevel;
				}

			}
		}
		if (WeaponInfo != null)
		{
			foreach (Requirement r in WeaponInfo.WearRequirements)
			{
				if (r.Skill == skillName)
				{
					return r.SkillLevel;
				}
			}
		}
		return 0;
	}
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
	public bool HasSkillRequirements()
	{
		foreach (Requirement r in Requirements)
		{
			if (r.Skill != "None" && r.IsMet() == false)
			{
				return false;
			}
		}
		return true;
	}
	public bool HasArmorRequirements()
    {
		if (ArmorInfo?.WearRequirements == null)
		{
			return true;
		}
		foreach (Requirement r in ArmorInfo.WearRequirements)
        {
			if(r.IsMet() == false)
            {
				return false;
            }
            }
        
		return true;
    }
	public bool HasWeaponRequirements()
    {
		if(WeaponInfo?.WearRequirements == null)
        {
			return true;
        }
		foreach (Requirement r in WeaponInfo.WearRequirements)
		{
			if (r.IsMet() == false)
			{
				return false;
			}
		}
		
		return true;
	}
	public string GetRequirementTooltip(bool showAll)
	{
		if (HasWeaponRequirements() && HasArmorRequirements() && HasRequirements() && !showAll)
		{
			return "";
		}
		string req = "";

		bool hasEquipInfo = false;
		if (ArmorInfo != null)
		{
			hasEquipInfo = true;
			foreach (Requirement r in ArmorInfo.WearRequirements)
			{
                if (showAll)
                {
					req += r.AllRequirementsString() + "\n";
                }
				else if (r.IsMet() == false)
				{
					req += r.ToString() + "\n";
				}
			}
		}
		if (WeaponInfo != null)
		{
			hasEquipInfo = true;
			foreach (Requirement r in WeaponInfo.WearRequirements)
			{
				if (showAll)
				{
					req += r.AllRequirementsString() + "\n";
				}
				else if (r.IsMet() == false)
				{
					req += r.ToString() + "\n";
				}
			}
		}
		if (hasEquipInfo == false)
		{
			foreach (Requirement r in Requirements)
			{
				if (showAll)
				{
					req += r.AllRequirementsString() + "\n";
				}
				else if (r.IsMet() == false)
				{
					req += r.ToString() + "\n";
				}
			}
		}
		if(req.Length > 0)
        {
			req = req.Substring(0, req.Length - 1);
		}
        else
        {
			return "This item has no requirements.";
        }
		return req;
	}
	public string GetRequirementTooltip()
    {
		return GetRequirementTooltip(false);
	}
	public string GetSkillForWeaponExp()
    {
		if(WeaponInfo == null)
        {
			Console.WriteLine("Weapon " + Name + " has no weaponinfo.");
			return "";
        }
		foreach(Requirement req in WeaponInfo.WearRequirements)
        {
			if(req.Skill != null)
            {
				return req.Skill;
            }
        }
		//Console.WriteLine("Weapon " + Name + " has no requirement for any skill, so no experience can be given.");
		return "";
    }

	public override string ToString()
	{
		return Name;
	}

	public void AddTag(string tag)
    {
		if(Tabs.Contains(tag) == false)
        {
			Tabs.Add(tag);
        }
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
		copy.Tabs = Tabs;
		copy.Parameter = Parameter;
		copy.Charges = Charges;
		copy.OpenCraftingMenuText = OpenCraftingMenuText;

		copy.Icon = Icon;
		copy.EquipSlot = EquipSlot;

		copy.IsStackable = IsStackable;
		copy.IsSellable = IsSellable;
		copy.IsLocked = IsLocked;
		copy.IsTome = IsTome;

		copy.Value = Value;

		copy.PrimaryColor = PrimaryColor;
		copy.SecondaryColor = SecondaryColor;

		copy.GatherSpeed = GatherSpeed;
		copy.ID = ID;
		copy.GatherSpeedBonus = GatherSpeedBonus;
		copy.ExperienceGainBonus = ExperienceGainBonus;
		copy.ExperienceBonusSkill = ExperienceBonusSkill;
		copy.ArmorInfo = ArmorInfo;
		copy.WeaponInfo = WeaponInfo;
		copy.AlchemyInfo = AlchemyInfo;
		copy.FoodInfo = FoodInfo;
		copy.TrapInfo = TrapInfo;
		copy.TanningInfo = TanningInfo;
		copy.Requirements = Requirements;
		return copy;
    }
}
