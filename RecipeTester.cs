using System;
using System.ComponentModel;
using System.Linq;

public class RecipeTester
{
	public void TestRecipes()
    {
        foreach(Recipe r in ItemManager.Instance.Recipes)
        {
            TestRecipe(r);
        }
    }

    private void TestRecipe(Recipe recipe)
    {
        Player.Instance.Inventory.Clear();
        Player.Instance.ResetStats();
        foreach(Requirement r in recipe.Requirements)
        {
            if(r.Skill != "None")
            {
                Player.Instance.Skills.FirstOrDefault(x => x.Name == r.Skill).SetSkillLevel(r.SkillLevel);
            }
        }
        foreach(Ingredient i in recipe.Ingredients)
        {
            Player.Instance.Inventory.AddMultipleOfItem(i.Item, i.Amount * recipe.MaxOutputsPerAction);
            Console.WriteLine("Adding " + i.Amount + " " + i.ItemName);
        }
        recipe.Create(out int created);
        Console.WriteLine("Created "+ created + " " + recipe.OutputItemName + ".");
    }
}
