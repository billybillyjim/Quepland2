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
    public async Task LoadItems(HttpClient Http)
    {
        Items.AddRange(await Http.GetJsonAsync<GameItem[]>("data/Items/General.json"));
        Items.AddRange(await Http.GetJsonAsync<Weapon[]>("data/Items/Weapons.json"));
        Items.AddRange(await Http.GetJsonAsync<Armor[]>("data/Items/Armors.json"));
    }


    public GameItem GetItemByName(string name)
    {
        return Items.FirstOrDefault(x => x.Name == name);
    }
}

