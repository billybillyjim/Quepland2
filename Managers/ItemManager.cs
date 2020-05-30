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
    public static List<string> FileNames = new List<string> { "Weapons", "Armors", "Sushi", "QuestItems", "General", "Fishing", "Ores", "WoodworkingItems", "Logs" };
    public static int baseID;
    public static readonly int MaxItemsPerFile = 100;
    public bool IsSelling = false;
    public int SellAmount = 1;
    public Shop CurrentShop;
    public async Task LoadItems(HttpClient Http)
    {
        foreach(string file in FileNames)
        {
            List<GameItem> addedItems = new List<GameItem>();
            addedItems.AddRange(await Http.GetJsonAsync<GameItem[]>("data/Items/" + file + ".json"));
            int iterator = baseID;
            foreach(GameItem i in addedItems)
            {
                i.ID = iterator;
                iterator++;
                i.Category = file;
            }
            Items.AddRange(addedItems);
            baseID += MaxItemsPerFile;
            
        }

        Recipes.AddRange(await Http.GetJsonAsync<Recipe[]>("data/Recipes/WoodworkingRecipes.json"));
        Recipes.AddRange(await Http.GetJsonAsync<Recipe[]>("data/Recipes/UnpackingRecipes.json"));
        Recipes.AddRange(await Http.GetJsonAsync<Recipe[]>("data/Recipes/SushiRecipes.json"));
    }


    public GameItem GetItemByName(string name)
    {
        return Items.FirstOrDefault(x => x.Name == name);
    }

    public Recipe GetUnpackingRecipe(GameItem item)
    {
        foreach(Recipe r in Recipes)
        {
            if(r.Ingredients.Count == 1 && r.Ingredients[0].Item == item.Name)
            {
                return r;
            }
        }
        return null;
    }

}

