using System;
using System.Collections.Generic;
using System.Linq;

public class DropTable
{
    public List<Drop> Drops { get; set; } = new List<Drop>();
    public List<string> AlwaysDrops { get; set; } = new List<string>();
    private static Random rand = new Random();

    public DropTable()
    {

    }

    public GameItem GetDrop()
    {
        if(Drops.Count == 0)
        {
            Console.WriteLine("DropTable unset for monster.");
            return null;
        }
        int size = Drops.Select(x => x.Weight).Sum();
        int roll = rand.Next(0, size);
        foreach(Drop drop in Drops)
        {
            if(roll <= drop.Weight)
            {
                MessageManager.AddMessage("You received a " + drop.ItemName);
                return ItemManager.Instance.GetItemByName(drop.ItemName);
            }
            roll -= drop.Weight;
        }
        return null;
    }
    public List<GameItem> GetAlwaysDrops()
    {
        List<GameItem> items = new List<GameItem>();
        foreach(string drop in AlwaysDrops)
        {
            items.Add(ItemManager.Instance.GetItemByName(drop));
        }
        return items;
    }

}
