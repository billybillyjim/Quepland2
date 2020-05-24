using System;

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
        foreach(Ingredient i in recipe.Ingredients)
        {
            Player.Instance.Inventory.AddMultipleOfItem(i.Item, i.Amount * recipe.OutputAmount);
        }
        recipe.Create(out int created);

    }
}
