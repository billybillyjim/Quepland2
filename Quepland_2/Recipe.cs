using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

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
            if(SecondaryOutputItemName == null)
            {
                return null;
            }
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
            if(TertiaryOutputItemName == null)
            {
                return null;
            }
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
            if(Player.Instance.Inventory.GetNumberOfItem(ingredient.Item) < ingredient.Amount)
            {
                ingredient.Item.Rerender = true;
                return false;
            }
        }
        return true;
    }
    public bool CanCreateFromInventory(Inventory inv)
    {
        if (HasSpace() == false)
        {
            return false;
        }
        else if (HasRequirements() == false)
        {
            return false;
        }
        foreach (Ingredient ingredient in Ingredients)
        {
            if (inv.GetNumberOfItem(ingredient.Item) < ingredient.Amount)
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
        if (Output.IsStackable)
        {
            if(Player.Instance.Inventory.GetAvailableSpaces() == 0 && Player.Instance.Inventory.HasItem(Output) == false && HasOnlyStackableIngredients())
            {
                return false;
            }
            
        }
        else if (Player.Instance.Inventory.GetAvailableSpaces() < OutputAmount * GetRequiredSpaces())
        {           
            return false;
        }
        return true;
    }
    public bool HasSomeIngredients()
    {
        foreach(Ingredient i in Ingredients)
        {
            if (Player.Instance.Inventory.GetNumberOfItem(i.Item) >= i.Amount)
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
            if (Player.Instance.Inventory.GetNumberOfItem(i.Item) < i.Amount)
            {
                ing += i.Amount + " " + i.Item + "\n";
            }
        }
        return ing;
    }
    public int GetNumberOfIngredients()
    {
        int totalSpaces = 0;
        foreach(Ingredient i in Ingredients)
        {
            if (i.Item.IsStackable)
            {
                totalSpaces++;
            }
            else
            {
                totalSpaces += i.Amount;
            }
        }
        return totalSpaces;
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

        req = req.Trim();
        return req;
    }
    public string GetIngredientsString()
    {
        string ing = "Ingredients:" + "\n";
        foreach (Ingredient i in Ingredients)
        {
                ing += i.Amount + " " + i.Item + "\n";           
        }
        ing = ing.Trim();
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
    public string GetIngredientsOnlyString()
    {
        if (Ingredients.Count == 1)
        {
            return Ingredients[0].ItemName;
        }
        else if (Ingredients.Count == 2)
        {
            return Ingredients[0].ItemName + " and " + Ingredients[1].ItemName;
        }
        else
        {
            string ing = "";
            foreach (Ingredient i in Ingredients)
            {
                ing += i.Item + ", ";
            }
            ing = ing.Substring(0, ing.Length - 2);
            return ing;
        }

    }
    public bool HasOnlyStackableIngredients()
    {
        foreach(Ingredient i in Ingredients)
        {
            if(i.Item.IsStackable == false)
            {
                return false;
            }
        }
        return true;
    }
    public int GetMaxOutput()
    {
        int max = MaxOutputsPerAction;
        
        foreach(Ingredient i in Ingredients)
        {
            if (i.DestroyOnUse)
            {
                max = Math.Min(MaxOutputsPerAction, Player.Instance.Inventory.GetNumberOfItem(i.Item) / i.Amount);
            }
        }
        return max;
    }
    /// <summary>
    /// Returns true if the item was sucessfully created. Outputs the number created for the message manager.
    /// </summary>
    /// <param name="created"></param>
    /// <returns></returns>
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
                    ingredient.Item.Rerender = true;
                    Player.Instance.Inventory.RemoveItems(ingredient.Item, ingredient.Amount * maxOutput);
                    
                }
            }
            Player.Instance.GainExperienceMultipleTimes(ExperienceGained, OutputAmount *  maxOutput);
            if (GameState.CurrentArtisanTask != null)
            {
                if (GameState.CurrentArtisanTask.ItemName == OutputItemName)
                {
                    if (long.TryParse(ExperienceGained.Split(':')[1], out long xp))
                    {
                        Player.Instance.GainExperience("Artisan", xp * OutputAmount * maxOutput / 5);
                    }
                    else
                    {
                        Player.Instance.GainExperience("Artisan", 15);
                    }
                }
            }
            Output.Rerender = true;
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
        int removedOnCreation = 0;
        foreach(Ingredient i in Ingredients)
        {
            if (i.Item.IsStackable == false && i.DestroyOnUse)
            {
                removedOnCreation++;
            }
        }
        if(TertiaryOutputItemName != null)
        {
            return Math.Max(0, 3 - removedOnCreation);
        }
        else if(SecondaryOutputItemName != null)
        {
            return Math.Max(0, 2 - removedOnCreation);
        }
        return Math.Max(0, 1 - removedOnCreation);
    }
    public int GetRequiredLevel(string skill)
    {
        foreach(Requirement r in Requirements)
        {
            if(r.Skill == skill)
            {
                return r.SkillLevel;
            }
        }
        return 0;
    }
    public int GetRequirementForSkill(string skillName)
    {
        foreach (Requirement r in Requirements)
        {
            if (r.Skill == skillName)
            {
                return r.SkillLevel;
            }
        }
        return 0;
    }
}
