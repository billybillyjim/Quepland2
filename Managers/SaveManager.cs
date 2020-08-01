using Microsoft.JSInterop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


public static class SaveManager
{
    public static string SaveVersion = "";
    public static DateTime LastSave;
    public static IJSRuntime jSRuntime;

    public static async Task SaveGame()
    {
        try
        {
            await SetItemAsync("Version", GameState.Version);
            await SetItemAsync("Playtime", GetSaveString(GameState.CurrentTick));
            await SetItemAsync("LastSave", DateTime.UtcNow);
            await SetItemAsync("Game Mode", GetSaveString(GameState.CurrentGameMode));
            await SetItemAsync("Skills", GetSkillsSave());
            await SetItemAsync("Inventory", GetItemSave(Player.Instance.Inventory));
            await SetItemAsync("Bank", GetItemSave(Bank.Instance.Inventory));
            await SetItemAsync("Areas", GetAreaSave());
            await SetItemAsync("Regions", GetRegionSave());
            await SetItemAsync("Quests", GetQuestSave());
            await SetItemAsync("GameState", JsonConvert.SerializeObject(GameState.GetSaveData()));
            await SetItemAsync("Player", JsonConvert.SerializeObject(Player.Instance.GetSaveData()));
            await SetItemAsync("Followers", FollowerManager.Instance.GetSaveData());

            LastSave = DateTime.UtcNow;
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to save.");
            Console.WriteLine(e.Message);
        }
    }
    public static async Task LoadSaveGame()
    {
        var serializerSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };
        Stopwatch watch = new Stopwatch();
        watch.Start();
        if(await ContainsKeyAsync("Playtime"))
        {
            GameState.CurrentTick = int.Parse(Decrypt(await GetItemAsync<string>("Playtime")));
        }
        watch.Stop();
        Console.WriteLine(watch.ElapsedMilliseconds);
        watch.Restart();
        if(await ContainsKeyAsync("Game Mode"))
        {
            string mode = Decrypt(await GetItemAsync<string>("Game Mode"));
            if(mode == "Normal")
            {
                GameState.CurrentGameMode = GameState.GameType.Normal;
            }
            else if(mode == "Hardcore")
            {
                GameState.CurrentGameMode = GameState.GameType.Hardcore;
            }
            else if(mode == "Ultimate")
            {
                GameState.CurrentGameMode = GameState.GameType.Ultimate;
            }
        }
        watch.Stop();
        Console.WriteLine(watch.ElapsedMilliseconds);
        watch.Restart();
        if (await ContainsKeyAsync("LastSave"))
        {
            LastSave = DateTime.Parse(await GetItemAsync<string>("LastSave"));
        }
        watch.Stop();
        Console.WriteLine(watch.ElapsedMilliseconds);
        watch.Restart();
        if (await ContainsKeyAsync("Skills"))
        {
            string[] data = (await GetItemAsync<string>("Skills")).Split(',');
            foreach(string d in data)
            {
                if(d.Length > 1)
                {
                    Player.Instance.Skills.Find(x => x.Name == d.Split(':')[0]).LoadExperience(long.Parse(d.Split(':')[1]));

                }
            }
        }
        watch.Stop();
        Console.WriteLine(watch.ElapsedMilliseconds);
        watch.Restart();
        if (await ContainsKeyAsync("Inventory"))
        {
            Player.Instance.Inventory.Clear();
            Player.Instance.Inventory.LoadData(await GetItemAsync<string>("Inventory"));
        }
        watch.Stop();
        Console.WriteLine(watch.ElapsedMilliseconds);
        watch.Restart();
        if (await ContainsKeyAsync("Bank"))
        {
            Bank.Instance.Inventory.Clear();
            Bank.Instance.Inventory.LoadData(await GetItemAsync<string>("Bank"));
        }
        watch.Stop();
        Console.WriteLine(watch.ElapsedMilliseconds);
        watch.Restart();
        if (await ContainsKeyAsync("Areas"))
        {
            AreaManager.Instance.LoadAreaSave(JsonConvert.DeserializeObject<List<AreaSaveData>>(await GetItemAsync<string>("Areas")));
        }
        watch.Stop();
        Console.WriteLine(watch.ElapsedMilliseconds);
        watch.Restart();
        if (await ContainsKeyAsync("Regions"))
        {
            AreaManager.Instance.LoadRegionSave(JsonConvert.DeserializeObject<List<RegionSaveData>>(await GetItemAsync<string>("Regions")));
        }
        watch.Stop();
        Console.WriteLine(watch.ElapsedMilliseconds);
        watch.Restart();
        if (await ContainsKeyAsync("Quests"))
        {
            QuestManager.Instance.LoadQuestSave(JsonConvert.DeserializeObject<List<QuestSaveData>>(await GetItemAsync<string>("Quests")));
        }
        watch.Stop();
        Console.WriteLine(watch.ElapsedMilliseconds);
        watch.Restart();
        if (await ContainsKeyAsync("GameState"))
        {
            GameState.LoadSaveData(JsonConvert.DeserializeObject<GameStateSaveData>(await GetItemAsync<string>("GameState")));
        }
        watch.Stop();
        Console.WriteLine(watch.ElapsedMilliseconds);
        watch.Restart();
        if (await ContainsKeyAsync("Followers"))
        {
            FollowerManager.Instance.LoadSaveData(await GetItemAsync<string>("Followers"));
        }
        watch.Stop();
        Console.WriteLine(watch.ElapsedMilliseconds);
        watch.Restart();
        if (await ContainsKeyAsync("Player"))
        {
            Player.Instance.LoadSaveData(JsonConvert.DeserializeObject<PlayerSaveData>(await GetItemAsync<string>("Player")));
        }
        watch.Stop();
        Console.WriteLine(watch.ElapsedMilliseconds);
        watch.Restart();
    }
    public static string GetItemSave(Inventory i)
    { 
        string data = "";
        foreach(KeyValuePair<GameItem, int> pair in i.GetItems())
        {
            data += pair.Key.UniqueID + "_" + pair.Value + "/";
        }
        return data;
    }
    public static string GetAreaSave()
    {
        return JsonConvert.SerializeObject(AreaManager.Instance.GetAreaSave());
    }
    public static string GetRegionSave()
    {
        return JsonConvert.SerializeObject(AreaManager.Instance.GetRegionSaveData());
    }
    public static string GetQuestSave()
    {
        return JsonConvert.SerializeObject(QuestManager.Instance.GetQuestSaveData());
    }
    public static string GetSkillsSave()
    {
        string s = "";
        foreach(Skill skill in Player.Instance.Skills)
        {
            s += skill.Name + ":" + skill.Experience + ",";
        }
        return s;
    }
    public static async Task<bool> HasSaveFile()
    {
        return await ContainsKeyAsync("Version");
    }
    public static string GetSaveString(Object o)
    {
        return Crypt(JsonConvert.SerializeObject(o));
    }
    private static byte[] key = new byte[8] { 1, 2, 3, 4, 5, 6, 7, 8 };
    private static byte[] iv = new byte[8] { 1, 2, 3, 4, 5, 6, 7, 8 };

    private static string Crypt(this string text)
    {
        SymmetricAlgorithm algorithm = DES.Create();
        ICryptoTransform transform = algorithm.CreateEncryptor(key, iv);
        byte[] inputbuffer = Encoding.Unicode.GetBytes(text);
        byte[] outputBuffer = transform.TransformFinalBlock(inputbuffer, 0, inputbuffer.Length);
        return Convert.ToBase64String(outputBuffer);
    }

    private static string Decrypt(this string text)
    {
        SymmetricAlgorithm algorithm = DES.Create();
        ICryptoTransform transform = algorithm.CreateDecryptor(key, iv);
        byte[] inputbuffer = Convert.FromBase64String(text);
        byte[] outputBuffer = transform.TransformFinalBlock(inputbuffer, 0, inputbuffer.Length);
        return Encoding.Unicode.GetString(outputBuffer);
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

