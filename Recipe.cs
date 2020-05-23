using System;
using System.Collections.Generic;

public class Recipe
{
    /// <summary>
    /// For json loading only, use Output to get the recipe output.
    /// </summary>
	public string OutputItemName { get; set; }
    public GameItem Output { get
        {
            return ItemManager.Instance.GetItemByName(OutputItemName);
        }
    }
    public List<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
    public int CraftingSpeed { get; set; } = 12;
    public int OutputAmount { get; set; } = 1;
    public int MaxOutputsPerAction { get; set; } = 1;

    /// <summary>
    /// Checks to see if the player has enough of each ingredient.
    /// </summary>
    /// <returns></returns>
	public bool CanCreate()
    {
        if((Output.IsStackable && (Player.Instance.Inventory.GetAvailableSpaces() == 0 && Player.Instance.Inventory.HasItem(Output) == false)) ||
            Player.Instance.Inventory.GetAvailableSpaces() < OutputAmount)
        {
            return false;
        }
		foreach(Ingredient ingredient in Ingredients)
        {
            if(Player.Instance.Inventory.GetNumberOfItem(ItemManager.Instance.GetItemByName(ingredient.Item)) < ingredient.Amount)
            {
                return false;
            }
        }
        return true;
    }
    public bool HasSomeIngredients()
    {
        foreach(Ingredient i in Ingredients)
        {
            if (Player.Instance.Inventory.GetNumberOfItem(ItemManager.Instance.GetItemByName(i.Item)) >= i.Amount)
            {
                return true;
            }
        }
        return false;
    }
    public string GetMissingIngredients()
    {
        string ing = "Missing:" + "\n";
        foreach (Ingredient i in Ingredients)
        {
            if (Player.Instance.Inventory.GetNumberOfItem(ItemManager.Instance.GetItemByName(i.Item)) < i.Amount)
            {
                ing += i.Amount + " " + i.Item + "\n";
            }
        }
        return ing;
    }
    public string GetIngredientsString()
    {
        string ing = "Ingredients:" + "\n";
        foreach (Ingredient i in Ingredients)
        {
                ing += i.Amount + " " + i.Item + "\n";           
        }
        return ing;
    }
    public int GetMaxOutput()
    {
        int max = MaxOutputsPerAction;
        
        foreach(Ingredient i in Ingredients)
        {
            if (i.DestroyOnUse)
            {
                max = Math.Min(MaxOutputsPerAction / i.Amount, Player.Instance.Inventory.GetNumberOfItem(ItemManager.Instance.GetItemByName(i.Item)));
                Console.WriteLine(Output.Name + ":Max Output:" + max);
                Console.WriteLine(i.Item + ":Player Has:" + Player.Instance.Inventory.GetNumberOfItem(ItemManager.Instance.GetItemByName(i.Item)));
                Console.WriteLine(i.Item + ":Max outputs:" + MaxOutputsPerAction);
            }
        }
        return max;
    }
    public bool Create(out int created)
    {
        created = 0;
        if (CanCreate())
        {
            int maxOutput = GetMaxOutput();
            foreach(Ingredient ingredient in Ingredients)
            {
                if (ingredient.DestroyOnUse)
                {
                    Player.Instance.Inventory.RemoveItems(ItemManager.Instance.GetItemByName(ingredient.Item), ingredient.Amount * maxOutput);
                    Console.WriteLine("Removing " + (ingredient.Amount * maxOutput) + " " + ingredient.Item);
                }
            }
            Player.Instance.GainExperienceMultipleTimes(Output.ExperienceGained, maxOutput);
            Player.Instance.Inventory.AddMultipleOfItem(Output, OutputAmount * maxOutput);

            created = maxOutput;
            return true;
        }
        else if ((Output.IsStackable && (Player.Instance.Inventory.GetAvailableSpaces() == 0 && Player.Instance.Inventory.HasItem(Output) == false)) ||
            Player.Instance.Inventory.GetAvailableSpaces() < OutputAmount)
        {
            MessageManager.AddMessage("You don't have enough inventory space to create this.");
            return false;
        }
        else
        {
            return false;
        }
    }
}
