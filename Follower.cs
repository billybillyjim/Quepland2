using System;
using System.Collections.Generic;

public class Follower
{
    public string Name { get; set; }
    public string AutoCollectMessage { get; set; }
    public string AutoCollectSkill { get; set; }
    public int AutoCollectLevel { get; set; }
    public int AutoCollectSpeed { get; set; }
    public bool IsUnlocked { get; set; }
    public bool IsBanking { get; set; }
    public int InventorySize { get; set; }
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
        foreach(KeyValuePair<GameItem, int> itemPair in Inventory.GetItems())
        {
            Bank.Instance.Inventory.AddMultipleOfItem(itemPair.Key, itemPair.Value);
        }
        Inventory.Clear();
        IsBanking = false;
    }
    public void WithdrawItem(GameItem item)
    {
        int amtToWithdraw = Math.Min(Inventory.GetAvailableSpaces(), Bank.Instance.Inventory.GetNumberOfItem(item));
        Inventory.AddMultipleOfItem(item, Bank.Instance.Inventory.RemoveItems(item, amtToWithdraw));
        IsBanking = false;
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
            if(req.Skill == AutoCollectSkill && req.SkillLevel > AutoCollectLevel)
            {
                return false;
            }
        }
        return true;
    }
}
