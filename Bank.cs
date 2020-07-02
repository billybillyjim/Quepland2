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
	public int Amount { get; set; } = 1;

	public void DepositAll(Inventory inv)
    {
		foreach(KeyValuePair<GameItem, int> pair in inv.GetItems())
        {
			Inventory.AddMultipleOfItem(pair.Key, pair.Value);
        }
		inv.Clear();
    }
}
