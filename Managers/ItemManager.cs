using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

public class ItemManager
{
    private static readonly ItemManager instance = new ItemManager();
    private ItemManager() { }
    static ItemManager() { }
    public static ItemManager Instance
    {
        get
        {
            return instance;
        }
    }
    public List<GameItem> Items = new List<GameItem>();
    public List<Recipe> Recipes = new List<Recipe>();
    public List<Recipe> SmithingRecipes = new List<Recipe>();
    public static List<string> FileNames = new List<string> { "Weapons", "Armors", "Sushi", "QuestItems", "General", "Elements", "Hunting", "Fishing", "Ores", "WoodworkingItems", "Logs" };
    public static int baseID;
    public static readonly int MaxItemsPerFile = 100;
    public bool IsSelling = false;
    public int SellAmount = 1;
    public Shop CurrentShop;
    public async Task LoadItems(HttpClient Http)
    {
        foreach (string file in FileNames)
        {
            List<GameItem> addedItems = new List<GameItem>();
            addedItems.AddRange(await Http.GetJsonAsync<GameItem[]>("data/Items/" + file + ".json"));
            int iterator = baseID;
            int count = 0;
            foreach (GameItem i in addedItems)
            {
                i.ID = iterator;
                iterator++;
                count++;
                i.Category = file;
            }
            if(count >= MaxItemsPerFile)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Warning:" + file + " has " + count + " items, which is over the expected " + MaxItemsPerFile + " items in its file.");
                Console.ForegroundColor = ConsoleColor.Black;
            }
            Items.AddRange(addedItems);
            baseID += MaxItemsPerFile;

        }

        Recipes.AddRange(await Http.GetJsonAsync<Recipe[]>("data/Recipes/WoodworkingRecipes.json"));
        Recipes.AddRange(await Http.GetJsonAsync<Recipe[]>("data/Recipes/UnpackingRecipes.json"));
        Recipes.AddRange(await Http.GetJsonAsync<Recipe[]>("data/Recipes/SushiRecipes.json"));
        Recipes.AddRange(await Http.GetJsonAsync<Recipe[]>("data/Recipes/MiscRecipes.json"));

        SmithingRecipes.AddRange(await Http.GetJsonAsync<Recipe[]>("data/Recipes/Smithing/BrassSmithingRecipes.json"));
        SmithingRecipes.AddRange(await Http.GetJsonAsync<Recipe[]>("data/Recipes/Smithing/CopperSmithingRecipes.json"));

    }


    public GameItem GetItemByName(string name)
    {
        return Items.FirstOrDefault(x => x.Name == name);
    }

    public Recipe GetUnpackingRecipe(GameItem item)
    {
        foreach (Recipe r in Recipes)
        {
            if (r.Ingredients.Count == 1 && r.Ingredients[0].Item == item.Name)
            {
                return r;
            }
        }
        return null;
    }
    public Recipe GetSmithingRecipeByIngredients(string ingredients)
    {
        foreach(Recipe recipe in SmithingRecipes)
        {
            if(recipe.GetShortIngredientsString() == ingredients)
            {
                return recipe;
            }
        }
        Console.WriteLine("Failed to find recipe with ingredients:" + ingredients);
        return null;
    }
    public Recipe GetSmithingRecipeByOutput(string output)
    {
        foreach (Recipe recipe in SmithingRecipes)
        {
            if (recipe.OutputItemName == output)
            {
                return recipe;
            }
        }
        Console.WriteLine("Failed to find recipe with output:" + output);
        return null;
    }
    public GameItem GetItemFromFormula(AlchemicalFormula formula)
    {
        double baseValue = formula.InputMetal.AlchemyInfo.QueplarValue * formula.LocationMultiplier;
        double elementalValue = formula.Element.AlchemyInfo.QueplarValue * formula.ActionMultiplier;
        double totalValue = baseValue + elementalValue;
        Console.WriteLine("Base Value:" + formula.InputMetal.AlchemyInfo.QueplarValue + " x " + formula.LocationMultiplier);
        Console.WriteLine("Elemental Value:" + formula.Element.AlchemyInfo.QueplarValue + " x " + formula.LocationMultiplier);
        Console.WriteLine("Total Value:" + baseValue + " + " + elementalValue);
        Console.WriteLine("Sum:" + totalValue);
        foreach (GameItem i in Items)
        {
            if (i.AlchemyInfo != null && i.SmithingInfo != null)
            {
                if(i.AlchemyInfo.QueplarValue == totalValue)
                {
                    return i;
                }
            }
        }
        return Items.Find(x => x.Name == "Alchemical Dust");
    }
}

