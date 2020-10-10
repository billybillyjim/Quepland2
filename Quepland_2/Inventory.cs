using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;



public class Inventory
{
    private List<KeyValuePair<GameItem, int>> items { get; set; }
    private Dictionary<string, int> itemLookupDic { get; set; }
    private int maxSize = 30;
    private readonly int maxValue = int.MaxValue - 1000000;
    private int totalItems { get; set; }
    public bool AllItemsStack;
    public bool IsLoadingSave = false;
    public bool SkipIndexing = false;
    public bool FixItems = false;
    public Inventory() 
    {
        items = new List<KeyValuePair<GameItem, int>>();
        itemLookupDic = new Dictionary<string, int>();
    }
    public Inventory(int max)
    {
        items = new List<KeyValuePair<GameItem, int>>();
        itemLookupDic = new Dictionary<string, int>();
        maxSize = max;
    }
    public Inventory(int max, bool itemsStack)
    {
        items = new List<KeyValuePair<GameItem, int>>();
        itemLookupDic = new Dictionary<string, int>();
        maxSize = max;
        AllItemsStack = itemsStack;
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
    /// Used for loading size of inventory after loading a save.
    /// </summary>
    /// <param name="size"></param>
    public void SetSize(int size)
    {
        maxSize = size;
    }
    /// <summary>
    /// Returns the amount of that specific item in the inventory.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public int GetNumberOfItem(GameItem item)
    {
        if(item == null)
        {
            return 0;
        }
        if(!HasItem(item))
        {
            return 0;
        }
        return itemLookupDic[item.UniqueID];
        if (item.IsStackable || AllItemsStack)
        {
            return itemLookupDic[item.UniqueID];
            //return items.FirstOrDefault(x => x.Key.UniqueID == item.UniqueID).Value;
        }
        return items.Count(x => x.Key.UniqueID == item.UniqueID);
    }
    public int GetNumberOfUnlockedItem(GameItem item)
    {
        if (item == null)
        {
            return 0;
        }
        if (!HasItem(item))
        {
            return 0;
        }
        if (item.IsStackable || AllItemsStack)
        {
            if (item.IsLocked)
            {
                return 0;
            }
            return itemLookupDic[item.UniqueID];
            // items.FirstOrDefault(x => x.Key.UniqueID == item.UniqueID).Value;
        }
        return items.Count(x => x.Key.UniqueID == item.UniqueID && x.Key.IsLocked == false);
    }
    /// <summary>
    /// Returns the total amount of item slots used in the inventory.
    /// </summary>
    /// <returns></returns>
    public int GetUsedSpaces()
    {
        int slotsUsed = 0;
        foreach(KeyValuePair<GameItem, int> pair in items)
        {
            if(pair.Equals(new KeyValuePair<GameItem, int>()) == false)
            {
                slotsUsed++;
            }
        }
        return slotsUsed;
    }
    public int GetAvailableSpaces()
    {
        if (maxSize - GetUsedSpaces() < 0)
        {
            return 0;
        }
        return maxSize - GetUsedSpaces();
    }
    public List<KeyValuePair<GameItem, int>> GetItems()
    {
        return items;
    }
    public List<KeyValuePair<GameItem, int>> GetUniqueItems()
    {
        return items.DistinctBy(x => x.Key.Name, null).ToList();
    }

    /// <summary>
    /// Checks if the inventory contains any items with the same name as the given GameItem.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool HasItem(GameItem item)
    {
        return HasItem(item.UniqueID, false);
        //return HasItem(item.Name);
    }    
    /// <summary>
    /// Checks if the inventory contains any items with the same name as the given string.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool HasItem(string itemName)
    {
        if (itemName == null)
        {
            return false;
        }
        if(itemLookupDic.TryGetValue(ItemManager.Instance.GetItemByName(itemName).UniqueID, out _))
        {
            return true;
        }
        else
        {
            //Console.WriteLine("No item " + itemName + " exists.");
            return false;
        }
        
    }
    public bool HasItem(string uniqueID, bool empty)
    {
        if (uniqueID == null)
        {
            //Console.WriteLine("Unique id was null");
            return false;
        }
        if(itemLookupDic.TryGetValue(uniqueID, out int amt))
        {
            return true;
        }
        //Console.WriteLine("Not Found:" + uniqueID);
        return false;
    }
    public GameItem FindItemCapableOfAction(string action)
    {
        foreach (KeyValuePair<GameItem, int> item in items)
        {
            if (item.Key.EnabledActions != null)
            {
                if (item.Key.EnabledActions.Contains(action))
                {
                    return item.Key;
                }
            }
        }
        return null;
    }
    public bool HasArrows()
    {
        foreach (KeyValuePair<GameItem, int> item in items)
        {
            if(item.Key.WeaponInfo != null)
            {
                if (item.Key.WeaponInfo.IsArrow)
                {
                    return true;
                }
            }
        }
        return false;
    }
    public GameItem GetStrongestArrow()
    {
        GameItem strongest = null;
        foreach (KeyValuePair<GameItem, int> item in items)
        {
            if (item.Key.WeaponInfo != null)
            {
                if (item.Key.WeaponInfo.IsArrow)
                {
                    if(strongest == null || strongest.WeaponInfo.Damage < item.Key.WeaponInfo.Damage)
                    {
                        strongest = item.Key;
                    }
                }
            }
        }
        return strongest;
    }
    /*
    public int GetCoins()
    {
        if(items.TryGetValue(ItemManager.Instance.GetItemByName("Coins"), out int val))
        {
            return val;
        }
        return 0;
    }*/
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
        if (item.IsStackable || AllItemsStack)
        {
            if (HasItem(item))
            {
                KeyValuePair<GameItem, int> pair = items.FirstOrDefault(x => x.Key.Name == item.Name);
                int oldAmt = pair.Value;
                items.Remove(pair);
                items.Add(new KeyValuePair<GameItem, int>(pair.Key, oldAmt + 1));
            }
            else
            {
                items.Add(new KeyValuePair<GameItem, int>(item, 1));
            }
        }
        else
        {
            items.Add(new KeyValuePair<GameItem, int>(item, 1));
        }
        UpdateItemCount();
        return true;
    }
    public bool AddItem(string itemName)
    {
        return AddItem(ItemManager.Instance.GetCopyOfItem(itemName));
    }
    public bool AddItems(List<GameItem> items)
    {
        foreach(GameItem i in items)
        {
            if(AddItem(i) == false)
            {
                return false;
            }
        }
        return true;
    }
    public bool AddMultipleOfItem(GameItem item, int amount)
    {
        if (item == null || (totalItems >= maxSize && (item.IsStackable == false || HasItem(item) == false)))
        {
            //Console.WriteLine("Item was null:"+(item == null) + " total surpassed max size:" + (totalItems >= maxSize));
            return false;
        }
        if (amount <= 0)
        {
            return false;
        }
        if (item.IsStackable || AllItemsStack)
        {
            return AddItemStackable(item, amount);
        }
        else
        {
            for (int i = 0; i < amount; i++)
            {
                if (AddItem(item.Copy()) == false)
                {
                    UpdateItemCount();
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
        if (amount <= 0)
        {
            return false;
        }
        if (HasItem(item))
        {
            if (item.IsStackable || AllItemsStack)
            {
                KeyValuePair<GameItem, int> pair = items.FirstOrDefault(x => x.Key.UniqueID == item.UniqueID);
                int oldAmt = pair.Value;
                items.Remove(pair);
                pair.Key.Rerender = true;
                items.Add(new KeyValuePair<GameItem, int>(pair.Key, oldAmt + amount));
                //Console.WriteLine("Adding item:" + item.Name + ", old:" + oldAmt + "/" + (oldAmt + amount));
            }
            else
            {
                UpdateItemCount();
                return false;
            }
        }
        else
        {
            if (item.IsStackable || AllItemsStack)
            {
                item.Rerender = true;
                //item.itemPos = inventorySlotPos;
                items.Add(new KeyValuePair<GameItem, int>(item, amount));
                //Console.WriteLine("Has item was false:" + item.Name + ", adding " + amount);
            }
            else
            {
                UpdateItemCount();
                return false;
            }
        }
        UpdateItemCount();
        return true;
    }
    
    public bool AddDrop(Drop drop)
    {
        if(drop == null || drop.ItemName == "Unset")
        {
            return false;
        }
        GameItem i = ItemManager.Instance.GetCopyOfItem(drop.ItemName, drop.ItemParameter);
        if (i.Category == "QuestItems")
        {
            if(Player.Instance.Inventory.HasItem(i) || Bank.Instance.Inventory.HasItem(i))
            {
                return false;
            }
        }
        return AddMultipleOfItem(i, drop.Amount);
    }
    /// <summary>
    /// Returns the number of items removed, 0 if none were removed.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="amount"></param>
    /// <returns></returns>
    public int RemoveItems(GameItem item, int amount)
    {
        if(amount <= 0 || item == null)
        {
            return 0;
        }
        int removed = 0;
        var removedItems = new HashSet<KeyValuePair<GameItem,int>>();
        for(int i = 0; i < items.Count; i++)
        {
            if(items[i].Key.UniqueID == item.UniqueID)
            {
                if (items[i].Key.IsLocked || items[i].Key.IsEquipped)
                {
                    continue;
                }
                KeyValuePair<GameItem, int> pair = items[i];
                if (items[i].Value > amount)
                {                   
                    int oldAmt = pair.Value;
                    items.Remove(pair);
                    items.Add(new KeyValuePair<GameItem, int>(pair.Key, oldAmt - amount));
                    UpdateItemCount();
                    return amount;
                }
                else if(items[i].Value <= amount)
                {
                    int val = items[i].Value;
                    removedItems.Add(items[i]);
                    removed += val;
                    if(removed >= amount)
                    {
                        itemLookupDic.Remove(items[i].Key.UniqueID);
                        items.RemoveAll(x => removedItems.Contains(x));                    
                        UpdateItemCount();
                        return removed;
                    }
                    else
                    {
                        UpdateItemCount();
                    }
                }
            }
        }
        items.RemoveAll(x => removedItems.Contains(x));
        UpdateItemCount();
        return removed;
    }
    public int RemoveUnlockedItems(GameItem item, int amount)
    {
        if (amount <= 0 || item == null)
        {
            return 0;
        }
        int removed = 0;
        var removedItems = new HashSet<KeyValuePair<GameItem, int>>();
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].Key.UniqueID == item.UniqueID && items[i].Key.IsLocked == false)
            {
                KeyValuePair<GameItem, int> pair = items[i];

                if (items[i].Value > amount)
                {
                    int oldAmt = pair.Value;
                    items.Remove(pair);
                    items.Add(new KeyValuePair<GameItem, int>(pair.Key, oldAmt - amount));
                    UpdateItemCount();
                    return amount;
                }
                else if (items[i].Value <= amount)
                {
                    int val = items[i].Value;
                    removedItems.Add(items[i]);
                    removed += val;
                    if (removed >= amount)
                    {
                        itemLookupDic.Remove(items[i].Key.UniqueID);
                        items.RemoveAll(x => removedItems.Contains(x));
                        UpdateItemCount();
                        return removed;
                    }
                }
            }
        }
        items.RemoveAll(x => removedItems.Contains(x));
        UpdateItemCount();
        return removed;
    }
    public bool RemoveRecipeItems(Recipe recipe)
    {
        if(recipe.CanCreate() == false)
        {
            return false;
        }
        foreach(Ingredient ingredient in recipe.Ingredients)
        {
            RemoveItems(ingredient.Item, ingredient.Amount);
        }

        UpdateItemCount();
        return true;
    }
    public bool RemoveRecipeItemsFromFollower(Recipe recipe)
    {
        if (recipe.CanCreateFromInventory(this) == false)
        {
            return false;
        }
        foreach (Ingredient ingredient in recipe.Ingredients)
        {
            RemoveItems(ingredient.Item, ingredient.Amount);
        }

        UpdateItemCount();
        return true;
    }
    public void Clear()
    {
        items.Clear();
        itemLookupDic.Clear();
        UpdateItemCount();
    }
    public void UpdateItemCount()
    {
        if (IsLoadingSave || SkipIndexing)
        {
            return;
        }
        totalItems = 0;
        //inventorySlotPos = 0;
        itemLookupDic.Clear();
        foreach (KeyValuePair<GameItem, int> item in items)
        {         
            if(item.Key != null)
            {
                item.Key.Rerender = true;

                if (itemLookupDic.TryGetValue(item.Key.UniqueID, out int v))
                {
                    itemLookupDic[item.Key.UniqueID] += item.Value;
                }
                else
                {
                    itemLookupDic.Add(item.Key.UniqueID, item.Value);
                }

                if (item.Key.IsStackable)
                {
                    totalItems += 1;
                }
                else
                {
                    totalItems += item.Value;
                }
            }
        }
        if (FixItems)
        {
            items.Clear();
            foreach(KeyValuePair<string, int> pair in itemLookupDic)
            {
                items.Add(new KeyValuePair<GameItem, int>(ItemManager.Instance.GetItemByUniqueID(pair.Key), pair.Value));
            }
            FixItems = false;
        }
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
            if (i.Key != null && i.Key.EnabledActions.Contains(action))
            {
                return true;
            }
        }
        return false;

    }
    public double GetTotalValue()
    {
        double total = 0;
        foreach (KeyValuePair<GameItem, int> i in items)
        {
            total += i.Key.Value * i.Value;
        }
        return total;
    }
    public void RerenderAll()
    {
        foreach (KeyValuePair<GameItem, int> i in items)
        {
            i.Key.Rerender = true;
        }
    }
    public void LoadData(string data)
    {
        string[] i = data.Split('/');
        if(i.Length == 0)
        {
            return;
        }
        IsLoadingSave = true;
        foreach(string line in i)
        {
            string[] s = line.Split('_');
            if(s.Length < 2)
            {
                continue;
            }
            string id = s[0];
            GameItem it = ItemManager.Instance.LoadItemByUniqueID(id);
            if (s.Length >= 3)
            {
                List<string> tabs = JsonConvert.DeserializeObject<List<string>>(s[2]);
                foreach (string tag in tabs)
                {
                    it.AddTag(tag);
                }
            }
            if (s.Length >= 4)
            {
                if (bool.TryParse(s[3], out bool res))
                {
                    it.IsLocked = res;
                }
            }
            if (int.TryParse(s[1], out int amt))
            {
                AddMultipleOfItem(it, amt);
            }
            else
            {
                Console.WriteLine("Error loading item in line:" + line);
            }
            if(it != null)
            {
                it.IsLocked = false;
            }

        }
        //FixItems = true;
        IsLoadingSave = false;
        UpdateItemCount();
    }

}

