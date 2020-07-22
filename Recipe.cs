using System;
using System.Collections.Generic;

public class Recipe
{
    /// <summary>
    /// For json loading only, use Output to get the recipe output.
    /// </summary>
	public string OutputItemName { get; set; }
    private GameItem output;
    public GameItem Output { 
        get
        {
            if(output == null)
            {
                output = ItemManager.Instance.GetItemByName(OutputItemName);
                if(output == null)
                {
                    Console.WriteLine(OutputItemName + " is not found in item.");
                }
            }
            return output;
        }
    }
    public string SecondaryOutputItemName { get; set; }
    private GameItem secondaryOutput;
    public GameItem SecondaryOutput
    {
        get
        {
            if (secondaryOutput == null)
            {
                secondaryOutput = ItemManager.Instance.GetItemByName(SecondaryOutputItemName);
            }
            return secondaryOutput;
        }
    }
    public string TertiaryOutputItemName { get; set; }
    private GameItem tertiaryOutput;
    public GameItem TertiaryOutput
    {
        get
        {
            if (tertiaryOutput == null)
            {
                tertiaryOutput = ItemManager.Instance.GetItemByName(TertiaryOutputItemName);
            }
            return tertiaryOutput;
        }
    }
    public List<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
    public int CraftingSpeed { get; set; } = 12;
    public int OutputAmount { get; set; } = 1;
    public int MaxOutputsPerAction { get; set; } = 1;
    public string RecipeActionString { get; set; } = "You set to work...";
    public string RecipeButtonString { get; set; } = "Unpack";
    public List<Requirement> Requirements { get; set; } = new List<Requirement>();
    public string ExperienceGained { get; set; } = "None";

    /// <summary>
    /// Checks to see if the player has enough of each ingredient.
    /// </summary>
    /// <returns></returns>
	public bool CanCreate()
    {
        if(HasSpace() == false)
        {
            return false;
        }
        else if(HasRequirements() == false)
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
    public bool HasRequirements()
    {
        foreach (Requirement r in Requirements)
        {
            if (r.IsMet() == false)
            {
                return false;
            }
        }
        return true;
    }
    public List<string> GetRequiredSkills()
    {
        List<string> reqSkills = new List<string>();
        foreach (Requirement r in Requirements)
        {
            if (r.Skill != "None")
            {
                reqSkills.Add(r.Skill);
            }
        }
        return reqSkills;
    }
    public bool HasSpace()
    {
        if ((Output.IsStackable && (Player.Instance.Inventory.GetAvailableSpaces() == 0 && Player.Instance.Inventory.HasItem(Output) == false)) ||
    Player.Instance.Inventory.GetAvailableSpaces() < OutputAmount * GetRequiredSpaces())
        {
            return false;
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
    public string GetRequirementTooltip()
    {
        if (HasRequirements())
        {
            return "";
        }
        string req = "";

            foreach (Requirement r in Requirements)
            {
                if (r.IsMet() == false)
                {
                    req += r.ToString() + "\n";
                }
            }
        
        req = req.Substring(0, req.Length - 1);
        return req;
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
    public string GetShortIngredientsString()
    {
        if(Ingredients.Count == 1)
        {
            return Ingredients[0].Amount + " " + Ingredients[0].Item;
        }
        else if(Ingredients.Count == 2)
        {
            return Ingredients[0].Amount + " " + Ingredients[0].Item + " and " + Ingredients[1].Amount + " " + Ingredients[1].Item;
        }
        else
        {
            string ing = "";
            foreach (Ingredient i in Ingredients)
            {
                ing += i.Amount + " " + i.Item + ", ";
            }
            ing = ing.Substring(0, ing.Length - 2);
            return ing;
        }

    }
    public int GetMaxOutput()
    {
        int max = MaxOutputsPerAction;
        
        foreach(Ingredient i in Ingredients)
        {
            if (i.DestroyOnUse)
            {
                max = Math.Min(MaxOutputsPerAction / i.Amount, Player.Instance.Inventory.GetNumberOfItem(ItemManager.Instance.GetItemByName(i.Item)));
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
                    //Console.WriteLine("Removing " + (ingredient.Amount * maxOutput) + " " + ingredient.Item);
                }
            }
            Player.Instance.GainExperienceMultipleTimes(ExperienceGained, OutputAmount *  maxOutput);
            Player.Instance.Inventory.AddMultipleOfItem(Output, OutputAmount * maxOutput);
            if(SecondaryOutputItemName != null)
            {                
                Player.Instance.Inventory.AddMultipleOfItem(SecondaryOutput, OutputAmount * maxOutput);
            }
            if(TertiaryOutputItemName != null)
            {              
                Player.Instance.Inventory.AddMultipleOfItem(TertiaryOutput, OutputAmount * maxOutput);
            }
            created = maxOutput;
            return true;
        }
        else if ((Output.IsStackable && (Player.Instance.Inventory.GetAvailableSpaces() == 0 && Player.Instance.Inventory.HasItem(Output) == false)) ||
            Player.Instance.Inventory.GetAvailableSpaces() < OutputAmount * GetRequiredSpaces())
        {
            MessageManager.AddMessage("You don't have enough inventory space to do this.");
            return false;
        }
        else
        {
            return false;
        }
    }
    public string GetOutputsString()
    {
        if (TertiaryOutputItemName != null)
        {
            return "You get $ " + OutputItemName + ", " + SecondaryOutputItemName + ", and " + TertiaryOutputItemName + ".";
        }
        else if (SecondaryOutputItemName != null)
        {
            return "You get $ " + OutputItemName + " and " + SecondaryOutputItemName + ".";
        }
        return "You get $ " + OutputItemName + ".";
    }
    private int GetRequiredSpaces()
    {
        if(TertiaryOutputItemName != null)
        {
            return 3;
        }
        else if(SecondaryOutputItemName != null)
        {
            return 2;
        }
        return 1;
    }
}
