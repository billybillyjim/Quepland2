using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;


public class Inventory
{
    private Dictionary<GameItem, int> items;
    private int maxSize = 30;
    private readonly int maxValue = int.MaxValue - 1000000;
    private int totalItems;
    public Inventory(int max)
    {
        items = new Dictionary<GameItem, int>();
        maxSize = max;
    }
    public void IncreaseMaxSizeBy(int increase)
    {
        maxSize += increase;
    }
    public void ResetMaxSize()
    {
        maxSize = 30;
    }    
    public int GetSize()
    {
        return maxSize;
    }
    /// <summary>
    /// Returns the amount of that specific item in the inventory.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public int GetNumberOfItem(GameItem item)
    {
        if(items.ContainsKey(item) == false)
        {
            return 0;
        }
        return items[item];
    }
    /// <summary>
    /// Returns the total amount of items in the inventory.
    /// </summary>
    /// <returns></returns>
    public int GetTotalNumberOfItems()
    {
        return items.Values.Sum();
    }
    public int GetAvailableSpaces()
    {
        if (maxSize - GetTotalNumberOfItems() < 0)
        {
            return 0;
        }
        return maxSize - GetTotalNumberOfItems();
    }
    public Dictionary<GameItem, int> GetItems()
    {
        return items;
    }
    public bool HasItem(GameItem item)
    {
        return (items.ContainsKey(item));
    }
    public int GetCoins()
    {
        if(items.TryGetValue(ItemManager.Instance.GetItemByName("Coins"), out int val))
        {
            return val;
        }
        return 0;
    }
    public bool AddItem(GameItem item)
    {
        //If the added item is null, the inventory is full and the item is not stackable, 
        //or the inventory is full and the item is stackable, but not already in the inventory
        if (item == null ||
          (totalItems >= maxSize && item.IsStackable == false) ||
          (totalItems >= maxSize && item.IsStackable == true && HasItem(item) == false))
        {
            UpdateItemCount();
            return false;
        }
        if (items.TryGetValue(item, out int amount))
        {
            items[item] = Math.Min(amount + 1, maxValue);
        }
        else
        {
            items[item] = 1;
        }
        UpdateItemCount();
        return true;
    }
    public bool AddItem(string itemName)
    {
        return AddItem(ItemManager.Instance.GetItemByName(itemName));
    }
    public bool AddMultipleOfItem(GameItem item, int amount)
    {
        if (totalItems >= maxSize && (item.IsStackable == false || HasItem(item) == false))
        {
            return false;
        }
        if (amount < 0)
        {
            amount = 0;
        }
        if (item.IsStackable)
        {
            return AddItemStackable(item, amount);
        }
        else
        {
            for (int i = 0; i < amount; i++)
            {
                if (AddItem(item) == false)
                {
                    return false;
                }
            }
        }
        UpdateItemCount();
        return true;
    }
    public bool AddMultipleOfItem(string itemName, int amount)
    {
        return AddMultipleOfItem(ItemManager.Instance.GetItemByName(itemName), amount);
    }
    public bool AddItemStackable(GameItem item, int amount)
    {
        if (totalItems >= maxSize && (item.IsStackable == false || HasItem(item) == false))
        {
            return false;
        }
        if (amount < 0)
        {
            amount = 0;
        }

        if (items.TryGetValue(item, out int current))
        {
            if (item.IsStackable)
            {
                if (amount > 0 && current + amount < current)
                {
                    return false;
                }
                else
                {
                    items[item] = current + amount;
                }

            }
            else
            {
                return false;
            }
        }
        else
        {
            if (item.IsStackable)
            {
                //item.itemPos = inventorySlotPos;
                items[item] = amount;
            }
            else
            {
                return false;
            }
        }
        UpdateItemCount();
        return true;
    }
    /// <summary>
    /// Returns the number of items removed, 0 if none were removed.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="amount"></param>
    /// <returns></returns>
    public int RemoveItems(GameItem item, int amount)
    {
        if (items.TryGetValue(item, out int currentAmount))
        {
            if (currentAmount >= amount)
            {
                items[item] = currentAmount - amount;

                if (items[item] <= 0)
                {
                    items.Remove(item);
                }
                UpdateItemCount();
                return amount;
            }

        }
        UpdateItemCount();
        return 0;
    }
    private void UpdateItemCount()
    {
        totalItems = 0;
        //inventorySlotPos = 0;

        foreach (KeyValuePair<GameItem, int> item in items)
        {
            //item.Key.itemPos = inventorySlotPos;
            if (item.Key.IsStackable)
            {
                totalItems += 1;
            }
            else
            {
                totalItems += item.Value;
            }
            //inventorySlotPos++;
        }
    }

    public List<GameItem> GetSmeltableItems()
    {
        return items.Keys.Where(x => x.SmithingInfo != null && x.SmithingInfo.SmeltsInto != null).ToList();
    }
    public List<GameItem> GetSmithableItems()
    {
        List<GameItem> smithables = new List<GameItem>();
        foreach(GameItem i in items.Keys)
        {
            if(i.SmithingInfo != null && i.SmithingInfo.SmithsInto != null)
            {
                smithables.AddRange(i.SmithingInfo.SmithsInto);
            }
            
        }
        return smithables;
    }
    public bool HasToolRequirement(GameItem item)
    {
        foreach(Requirement r in item.Requirements)
        {
            if(r.Action != "None")
            {
                bool hasReq = true;
                foreach (KeyValuePair<GameItem, int> i in items)
                {
                    //item.Key.itemPos = inventorySlotPos;
                    if (i.Key.EnabledActions.Contains(r.Action) == false)
                    {
                        hasReq = false;
                    }
                }
                return hasReq;
            }
        }

        return true;
    }
    public bool HasToolRequirement(string action)
    {
        if (action == "None")
        {
            return true;
        }
        foreach (KeyValuePair<GameItem, int> i in items)
        {
            //item.Key.itemPos = inventorySlotPos;
            if (i.Key.EnabledActions.Contains(action))
            {
                return true;
            }
        }
        return false;

    }
}

