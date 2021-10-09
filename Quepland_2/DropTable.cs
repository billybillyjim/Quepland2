using System;
using System.Collections.Generic;
using System.Linq;

public class DropTable
{
    public List<Drop> Drops { get; set; } = new List<Drop>();
    public List<Drop> AlwaysDrops { get; set; } = new List<Drop>();
    private static Random rand = new Random();

    public DropTable()
    {

    }

    public List<Drop> GetDropsWithName(string itemName)
    {
        List<Drop> drops = new List<Drop>();
        foreach(Drop d in Drops)
        {
            if(d.ItemName == itemName)
            {
                drops.Add(d);
            }
        }
        return drops;
    }
    public Drop GetDrop()
    {
        if(Drops.Count == 0)
        {
            Console.WriteLine("DropTable unset for monster.");
            return null;
        }
        int size = Drops.Select(x => x.Weight).Sum();
        int roll = rand.Next(0, size + 1);
        foreach(Drop drop in Drops)
        {
            if(roll <= drop.Weight)
            {
               //MessageManager.AddMessage("You received " + drop.Amount + " " + drop.ItemName + ".");
              
                return drop;
            }
            roll -= drop.Weight;
        }
        return null;
    }
    public Drop GetDropWithRequirements()
    {
        if (Drops.Count == 0)
        {
            Console.WriteLine("DropTable unset for monster.");
            return null;
        }
        int size = Drops.Select(x => x.Weight).Sum();
        int roll = rand.Next(0, size + 1);
        foreach (Drop drop in Drops)
        {
            if (roll <= drop.Weight)
            {
                //MessageManager.AddMessage("You received " + drop.Amount + " " + drop.ItemName + ".");
                if (drop.Item.HasRequirements())
                {
                    return drop;
                }
                
            }
            roll -= drop.Weight;
        }
        return null;
    }
    public bool HasDrop(GameItem i)
    {
        if(i == null)
        {
            Console.WriteLine("Item was null.");
            return false;
        }
        foreach(Drop d in Drops)
        {
            if(d == null || d.Item == null)
            {
                Console.WriteLine("Drop was null:"+i.Name);
                return false;
            }
            if(d.Item.UniqueID == i.UniqueID)
            {
                return true;
            }
        }
        return false;
    }
}
