using System;
using System.Collections.Generic;
using System.Diagnostics;

public class Follower
{
    public string Name { get; set; }
    public string AutoCollectMessage { get; set; }
    public string AutoCollectSkill { get; set; }

    public int AutoCollectLevel { get; set; }
    private int autoCollectSpeed;
    public int AutoCollectSpeed { get 
        {
            return autoCollectSpeed;
        }
        set
        {
            autoCollectSpeed = value;
        } 
    }
    public bool IsUnlocked { get; set; }
    public bool IsBanking { get; set; }
    public int InventorySize { get; set; }
    public Skill Banking { get; set; } = new Skill()
    {
        Name = "Banking",
        Level = 1,
        Description = "Banking is the ability of your follower to properly manage inventory space and movement to transport goods from you to the bank efficiently."
    };
    private Inventory inv;
    public Inventory Inventory
    {
        get
        {
            if (inv == null)
            {
                inv = new Inventory(InventorySize);
            }
            return inv;
        }
    }
    public int TicksToNextAction { get; set; }

    public void BankItems()
    {
        Bank.Instance.Inventory.SkipIndexing = true;
        foreach(KeyValuePair<GameItem, int> itemPair in Inventory.GetItems())
        {
            Bank.Instance.Inventory.AddMultipleOfItem(itemPair.Key.Copy(), itemPair.Value);
            GainExperience(itemPair.Value);
        }
        Bank.Instance.Inventory.SkipIndexing = false;
        Bank.Instance.Inventory.FixItems = true;
        Bank.Instance.Inventory.UpdateItemCount();
        Inventory.Clear();
        IsBanking = false;

    }

    public void GainExperience(long amount)
    {

        if (Banking.GetSkillLevelUnboosted() >= AutoCollectLevel)
        {
            return;
        }
        Banking.Experience += amount;

        if (Banking.Experience >= (long)Skill.GetExperienceRequired(Banking.GetSkillLevelUnboosted()))
        {
            LevelUp();
        }

    }
    public void LevelUp()
    {
        if(Banking.GetSkillLevelUnboosted() >= AutoCollectLevel)
        {
            return;
        }
        if(Banking.GetSkillLevelUnboosted() % 2 == 0)
        {
            MessageManager.AddMessage(Name + " leveled up! Their banking level is now " + Banking.GetSkillLevelUnboosted() + ".");
            InventorySize++;
            Inventory.IncreaseMaxSizeBy(1);
        }
        else
        {
            MessageManager.AddMessage(Name + " leveled up!");
        }

        Banking.SetSkillLevel(Banking.GetSkillLevelUnboosted() + 1);
        
        if (Banking.Experience >= Skill.GetExperienceRequired(Banking.GetSkillLevelUnboosted()))
        {
            LevelUp();

        }
    }

    public void SendToBank()
    {
        IsBanking = true;
        TicksToNextAction = AutoCollectSpeed;
    }

    public bool MeetsRequirements(GameItem item)
    {
        if(item == null)
        {
            return false;
        }
        if(item.Requirements == null || item.Requirements.Count == 0)
        {
            return true;
        }
        foreach(Requirement req in item.Requirements)
        {
            if(req.Skill != "None" && req.Skill != AutoCollectSkill)
            {
                return false;
            }
            if(req.Skill == AutoCollectSkill && req.SkillLevel > AutoCollectLevel + Banking.GetSkillLevel())
            {
                return false;
            }
        }
        return true;
    }
}
