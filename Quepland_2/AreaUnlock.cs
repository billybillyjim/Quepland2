using System;
using System.Collections.Generic;

public class AreaUnlock
{
	public string AreaURL { get; set; }
	public string ButtonText { get; set; }
	public List<Requirement> Requirements { get; set; } = new List<Requirement>();
    public bool ConnectsLands { get; set; }
    public bool ConsumeRequiredItems { get; set; }
	public bool HasRequirements()
    {
		foreach(Requirement r in Requirements)
        {
			if(r.IsMet() == false)
            {
				return false;
            }
        }     
		return true;
    }
    public override string ToString()
    {
        string req = "";
        foreach(Requirement r in Requirements)
        {
            if(r.IsMet() == false)
            {
                req += r.ToString() + "\n";
            }        
        }
        req = req.Substring(0, req.Length - 1);
        return req;
    }
    public bool CheckItems()
    {
        if (ConsumeRequiredItems)
        {
            foreach (Requirement r in Requirements)
            {
                if (r.Item != "None")
                {
                    if (Player.Instance.Inventory.GetNumberOfItem(ItemManager.Instance.GetItemByName(r.Item)) < r.ItemAmount)
                    {
                        if (r.ItemAmount == 1)
                        {
                            MessageManager.AddMessage("You need a " + r.Item + ".", "red");
                        }
                        else
                        {
                            MessageManager.AddMessage("You don't have enough " + r.Item + ".(" + r.ItemAmount + ")", "red");
                        }

                        return false;
                    }
                }
            }
        }
        return true;
    }
    public bool RemoveItems()
    {
        if (CheckItems())
        {
            foreach (Requirement r in Requirements)
            {
                if (r.Item != "None")
                {
                    Player.Instance.Inventory.RemoveItems(ItemManager.Instance.GetItemByName(r.Item), r.ItemAmount);

                }
            }
        }
        else
        {
            return false;
        }
        return true;
    }
}
