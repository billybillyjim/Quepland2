using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


public static class SaveManager
{
    public static string SaveVersion = "";
    public static IJSRuntime jSRuntime;

    public static async Task SaveGame()
    {
        await SetItemAsync("Guid", GameState.Guid.ToString());
    }
    public async static Task SaveGuid()
    {
        await SetItemAsync("Guid", GameState.Guid.ToString());
    }
    public async static Task GetGuid()
    {
        if (await ContainsKeyAsync("Guid"))
        {
            GameState.Guid = Guid.Parse(await GetItemAsync<string>("Guid"));
        }
    }

    public async static Task SetItemAsync(string key, object data)
    {
        if (key == null || key.Length == 0)
        {
            return;
        }
        await jSRuntime.InvokeVoidAsync("localStorage.setItem", key, data);
    }
    public async static Task<string> GetItemAsync<T>(string key)
    {
        if (key == null || key.Length == 0)
        {
            throw new ArgumentNullException(nameof(key));
        }
        string data = await jSRuntime.InvokeAsync<string>("localStorage.getItem", key);
        if (data == null || data.Length == 0)
        {
            return "";
        }
        return data;
    }
    public async static Task<bool> ContainsKeyAsync(string key)
    {
        try
        {
            return await jSRuntime.InvokeAsync<bool>("localStorage.hasOwnProperty", key);
        }
        catch
        {
            return false;
        }
    }
}

