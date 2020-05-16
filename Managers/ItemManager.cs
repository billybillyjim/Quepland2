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
    public async Task LoadItems(HttpClient Http)
    {
        Items.AddRange(await Http.GetJsonAsync<GameItem[]>("data/Items/General.json"));
        Items.AddRange(await Http.GetJsonAsync<GameItem[]>("data/Items/Weapons.json"));
        Items.AddRange(await Http.GetJsonAsync<GameItem[]>("data/Items/Armors.json"));
        Items.AddRange(await Http.GetJsonAsync<GameItem[]>("data/Items/WoodworkingItems.json"));
        Items.AddRange(await Http.GetJsonAsync<GameItem[]>("data/Items/Ores.json"));

        Recipes.AddRange(await Http.GetJsonAsync<Recipe[]>("data/Recipes/WoodworkingRecipes.json"));
    }


    public GameItem GetItemByName(string name)
    {
        return Items.FirstOrDefault(x => x.Name == name);
    }
}

