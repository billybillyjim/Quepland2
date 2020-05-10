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
    public bool HasToolRequirement(GameItem item)
    {
        foreach (KeyValuePair<GameItem, int> i in items)
        {
            //item.Key.itemPos = inventorySlotPos;
            if (i.Key.EnabledActions.Contains(item.RequiredAction))
            {
                return true;
            }
        }
        return false;
    }
    public bool HasToolRequirement(string action)
    {
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

