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

}
