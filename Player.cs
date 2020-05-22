using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

public class Player
{
    private static readonly Player instance = new Player();
    private Player() { }
    static Player() { }
    public static Player Instance
    {
        get
        {
            return instance;
        }
    }
    public string Name { get; set; } = "";
    public Inventory Inventory = new Inventory(30);
    private List<GameItem> equippedItems = new List<GameItem>();
    public List<Skill> Skills = new List<Skill>();
    public Follower CurrentFollower { get; set; }
    public int MaxHP = 50;
    public int CurrentHP;
    public int TicksToNextAttack;

    public Skill LastGainedExp { get; set; }
    public Skill ExpTrackerSkill { get; set; }
    
    

    public async Task LoadSkills(HttpClient Http)
    {
        Skills.AddRange(await Http.GetJsonAsync<Skill[]>("data/Skills.json"));
    }
    private void IncreaseMaxHPBy(int amount)
    {
        MaxHP += amount;
    }
    public void GainExperience(string skill, long amount)
    {
        Skill s = Skills.FirstOrDefault(x => x.Name == skill);
        if (s != null)
        {
            GainExperience(s, amount);
        }
    }
    public void GainExperience(string skillAndExp)
    {
        if (string.IsNullOrEmpty(skillAndExp) || skillAndExp == "None")
        {
            return;
        }
        if (int.TryParse(skillAndExp.Split(':')[1], out int amount))
        {
            GainExperience(Skills.FirstOrDefault(x => x.Name == skillAndExp.Split(':')[0]), amount);
        }
    }
    public double GetGearMultiplier(GameItem item)
    {
        if(item.Requirements == null || item.Requirements.Count == 0)
        {
            return 0;
        }
        string skill = item.Requirements.FirstOrDefault(x => x.Skill != "None").Skill;
        double multi = 1;
        foreach(GameItem i in equippedItems)
        {
            multi -= i.GatherSpeedBonus;
        }
        return Math.Max(multi, 0.01);
    }
    public void Equip(GameItem item)
    {
        UnequipItem(equippedItems.Find(x => x.EquipSlot == item.EquipSlot));
        equippedItems.Add(item);
        item.IsEquipped = true;
    }
    public void UnequipItem(GameItem item)
    {
        if (item != null)
        {
            item.IsEquipped = false;
            equippedItems.Remove(item);
        }
    }
    public int GetTotalDamage()
    {
        int total = 0;
        total += Skills.Find(x => x.Name == "Strength").GetSkillLevel() * 3;
        foreach(GameItem item in equippedItems)
        {
            if(item.WeaponInfo != null)
            {
                total += item.WeaponInfo.Damage;
                total += Skills.Find(x => x.Name == item.GetSkillForWeaponExp()).GetSkillLevel() * 3;
            }
            if(item.ArmorInfo != null)
            {
                total += item.ArmorInfo.Damage;
            }
        }
        return total;
    }
    public GameItem GetWeapon()
    {
        return equippedItems.Find(x => x.EquipSlot == "Right Hand");
    }
    public int GetWeaponAttackSpeed()
    {
        GameItem weapon = GetWeapon();
        if (weapon != null && weapon.WeaponInfo != null)
        {
            return Math.Max(4, GetWeapon().WeaponInfo.AttackSpeed - (GetLevel("Deftness") / 15));
        }
        else
        {
            return Math.Max(8, 12 - (GetLevel("Deftness") / 15));
        }
    }
    public int GetLevel(string skillName)
    {
        foreach (Skill skill in Skills)
        {
            if (skill.Name == skillName)
            {
                return skill.GetSkillLevel();
            }
        }
        return 0;
    }
    public void GainExperience(Skill skill, long amount)
    {
        if (skill == null)
        {
            Console.WriteLine("Player gained " + amount + " experience in unfound skill.");
            return;
        }
        if (amount <= 0)
        {
            return;
        }
        LastGainedExp = skill;

        skill.Experience += (long)(amount);
        

        if (skill.Experience >= Skill.GetExperienceRequired(skill.GetSkillLevelUnboosted()))
        {
            LevelUp(skill);
        }
    }
    public void LevelUp(Skill skill)
    {
        skill.SetSkillLevel(skill.GetSkillLevelUnboosted() + 1);
        MessageManager.AddMessage("You leveled up! Your " + skill.Name + " level is now " + skill.GetSkillLevelUnboosted() + ".");
        if (skill.Name == "Strength")
        {
            Inventory.IncreaseMaxSizeBy(1);

            if (skill.GetSkillLevelUnboosted() % 10 == 0)
            {
                Inventory.IncreaseMaxSizeBy(4);
                MessageManager.AddMessage("You feel stronger. You can now carry 5 more items in your inventory.");
            }
            else
            {
                MessageManager.AddMessage("You feel stronger. You can now carry 1 more item in your inventory.");
            }
        }
        else if (skill.Name == "HP")
        {
            IncreaseMaxHPBy(5);
            if (skill.GetSkillLevelUnboosted() % 5 == 0)
            {
                IncreaseMaxHPBy(10);
                MessageManager.AddMessage("You feel much healthier. Your maximum HP has increased by 15!");
            }
            else
            {
                MessageManager.AddMessage("You feel healthier. Your maximum HP has increased by 5.");
            }
        }

        if (skill.Experience >= Skill.GetExperienceRequired(skill.GetSkillLevelUnboosted()))
        {
            LevelUp(skill);

        }
    }

    public bool HasSkillRequirement(string skill, int lvl)
    {
        Skill s = Skills.FirstOrDefault(x => x.Name == skill);
        if (s == null)
        {
            Console.WriteLine("Failed to find game item:" + skill);
            return false;
        }
        return s.Level >= lvl;
    }
    public bool HasToolRequirement(GameItem item)
    {
        return Inventory.HasToolRequirement(item);
    }
    public bool HasToolRequirement(string action)
    {
        return Inventory.HasToolRequirement(action);
    }
}

