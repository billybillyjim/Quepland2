using System;
using System.Collections.Generic;

public class Bank
{
	private static readonly Bank instance = new Bank();
	private Bank() { }
	static Bank() { }
    public List<string> Tabs = new List<string>() { "Main" };
    public string CurrentTab = "Main";
	public static Bank Instance { get
        {
			return instance;
        } 
	}
	public Inventory Inventory = new Inventory(int.MaxValue, true);
	public bool IsBanking { get; set; }
    public bool HasChanged { get; set; }
    private int amount = 1;
	public int Amount { 
        get { return amount;  } 
        set { 
            if (value < 0) { amount = 0; }
            else { amount = value; }
        } }

	public void DepositAll(Inventory inv)
    {
        List<KeyValuePair<GameItem, int>> lockedItems = new List<KeyValuePair<GameItem, int>>();
        List<GameItem> lockedEquippedItems = new List<GameItem>();
        foreach (KeyValuePair<GameItem, int> pair in inv.GetItems())
        {
            if (pair.Key.IsLocked)
            {
                if (pair.Key.IsEquipped)
                {                 
                    lockedEquippedItems.Add(pair.Key);
                }
                lockedItems.Add(new KeyValuePair<GameItem, int>(pair.Key, pair.Value));
            }
            else
            {
                if (pair.Key.IsEquipped)
                {
                    Player.Instance.Unequip(pair.Key);
                }
                
                Inventory.AddMultipleOfItem(pair.Key, pair.Value);
            }

        }
        inv.Clear();
        foreach (KeyValuePair<GameItem, int> pair in lockedItems)
        {
            pair.Key.IsLocked = true;
            inv.AddMultipleOfItem(pair.Key, pair.Value);
        }
        foreach(GameItem i in lockedEquippedItems)
        {
            i.IsLocked = true;
            Player.Instance.Equip(i.Name);
        }
        /*if (inv == Player.Instance.Inventory)
        {

            Player.Instance.GetEquippedItems().Clear();
            foreach (GameItem i in lockedEquippedItems)
            {
                
                Player.Instance.Equip(i);
            }
        }*/
    }
	public void Deposit(GameItem item)
    {
        Deposit(item, Amount);
    }
    public void Deposit(GameItem item, int amount)
    {
        if (item == null)
        {
            return;
        }
        if (item.IsEquipped)
        {
            MessageManager.AddMessage("You'll need to unequip this item before banking it.");
            return;
        }
        if (item.IsLocked)
        {
            MessageManager.AddMessage("You'll need to unlock this item before banking it.");
            return;
        }
        int amountToBank = Math.Min(Player.Instance.Inventory.GetNumberOfItem(item), amount);
        
        Inventory.AddMultipleOfItem(item.Copy(), Player.Instance.Inventory.RemoveItems(item, amountToBank));
        HasChanged = true;
    }
    public void Withdraw(GameItem item)
    {
        Withdraw(item, Amount);
    }
    public void Withdraw(GameItem item, int amount)
    {

        if (item == null)
        {
            return;
        }
        int maxWithdraw = Math.Max(0, amount);
        //If the item is stackable and the player has a stack in their inventory already, or the player has space for another item
        if ((item.IsStackable && Player.Instance.Inventory.HasItem(item) && Player.Instance.Inventory.GetAvailableSpaces() == 0) ||
         (item.IsStackable && Player.Instance.Inventory.GetAvailableSpaces() > 0))
        {
            maxWithdraw = Math.Min(amount, Inventory.GetNumberOfItem(item));
        }
        else
        {
            //Gets the smallest of the amount, inventory space, and number in the bank.
            maxWithdraw = Math.Min(Math.Min(amount, Player.Instance.Inventory.GetAvailableSpaces()), Inventory.GetNumberOfItem(item));
        }
        if (Player.Instance.Inventory.AddMultipleOfItem(item.Copy(), maxWithdraw))
        {
            Inventory.RemoveItems(item, maxWithdraw);
        }

    }
    public void LoadTabs(List<string> t)
    {
        Tabs = t;
    }

    public string GetAmountString()
    {
        if(Amount == int.MaxValue)
        {
            return "All";
        }
        return "" + Amount;
    }
}
