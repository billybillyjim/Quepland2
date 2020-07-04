using System;
using System.Collections.Generic;

public class Bank
{
	private static readonly Bank instance = new Bank();
	private Bank() { }
	static Bank() { }
	public static Bank Instance { get
        {
			return instance;
        } 
	}
	public Inventory Inventory = new Inventory(int.MaxValue);
	public bool IsBanking { get; set; }
    private int amount = 1;
	public int Amount { 
        get { return amount;  } 
        set { 
            if (value < 0) { amount = 0; }
            else { amount = value; }
        } }

	public void DepositAll(Inventory inv)
    {
		foreach(KeyValuePair<GameItem, int> pair in inv.GetItems())
        {
            pair.Key.IsEquipped = false;
			Inventory.AddMultipleOfItem(pair.Key, pair.Value);
        }
        Player.Instance.GetEquippedItems().Clear();

		inv.Clear();
    }
	public void Deposit(GameItem item)
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
        int amountToBank = Math.Min(Player.Instance.Inventory.GetNumberOfItem(item), Bank.Instance.Amount);
        Bank.Instance.Inventory.AddMultipleOfItem(item, amountToBank);
        Player.Instance.Inventory.RemoveItems(item, amountToBank);
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
