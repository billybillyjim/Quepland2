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

    /// <summary>
    /// Checks to see if the player has enough of each ingredient.
    /// </summary>
    /// <returns></returns>
	public bool CanCreate()
    {
		foreach(Ingredient ingredient in Ingredients)
        {
            if(Player.Instance.Inventory.GetNumberOfItem(ItemManager.Instance.GetItemByName(ingredient.Item)) < ingredient.Amount)
            {
                return false;
            }
        }
        return true;
    }
    public bool Create()
    {
        if (CanCreate())
        {
            foreach(Ingredient ingredient in Ingredients)
            {
                if (ingredient.DestroyOnUse)
                {
                    Player.Instance.Inventory.RemoveItems(ItemManager.Instance.GetItemByName(ingredient.Item), ingredient.Amount);
                }
            }
            Player.Instance.GainExperience(Output.ExperienceGained);
            Player.Instance.Inventory.AddItem(Output);
            return true;
        }
        else
        {
            return false;
        }
    }
}
