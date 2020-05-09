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
    public Inventory Inventory = new Inventory(30);
    private List<GameItem> equippedItems = new List<GameItem>();
    public List<Skill> Skills = new List<Skill>();
    public int MaxHP = 50;
    public string LastLevelledSkill;
    public bool LastLevelledSkillLocked;
    

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
        if (string.IsNullOrEmpty(skillAndExp))
        {
            return;
        }
        if (int.TryParse(skillAndExp.Split(':')[1], out int amount))
        {
            GainExperience(Skills.FirstOrDefault(x => x.Name == skillAndExp.Split(':')[0]), amount);
        }
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
        if (LastLevelledSkillLocked == false)
        {
            LastLevelledSkill = skill.Name;
        }

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
    public bool HasSkillRequirement(string itemName)
    {
        GameItem i = ItemManager.Instance.GetItemByName(itemName);
        if(i == null)
        {
            Console.WriteLine("Failed to find game item:" + itemName);
            return false;
        }
        return HasSkillRequirement(i);
    }
    public bool HasSkillRequirement(string skill, int lvl)
    {
        Skill s = Skills.FirstOrDefault(x => x.Name == skill);
        if(s == null)
        {
            Console.WriteLine("Failed to find game item:" + skill);
            return false;
        }
        return s.Level >= lvl;
    }
    public bool HasSkillRequirement(GameItem item)
    {
        if(item.RequiredAction == null || item.RequiredAction == "")
        {
            return true;
        }
        return HasSkillRequirement(item.RequiredAction, item.RequiredLevel);
    }
    public bool HasToolRequirement(GameItem item)
    {
        return Inventory.HasToolRequirement(item);
    }
}

