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
            string mode = GameState.CurrentGameMode.ToString();
            await SetItemAsync("Version:" + mode, GameState.Version);
            await SetItemAsync("Playtime:" + mode, GetSaveString(GameState.CurrentTick));
            await SetItemAsync("LastSave:" + mode, DateTime.UtcNow);
            await SetItemAsync("Skills:" + mode, GetSkillsSave());
            await SetItemAsync("Inventory:" + mode, GetItemSave(Player.Instance.Inventory));
            await SetItemAsync("Bank:" + mode, GetItemSave(Bank.Instance.Inventory));
            await SetItemAsync("Areas:" + mode, GetAreaSave());
            await SetItemAsync("Regions:" + mode, GetRegionSave());
            await SetItemAsync("Quests:" + mode, GetQuestSave());
            await SetItemAsync("GameState:" + mode, JsonConvert.SerializeObject(GameState.GetSaveData()));
            await SetItemAsync("Player:" + mode, JsonConvert.SerializeObject(Player.Instance.GetSaveData()));
            await SetItemAsync("Followers:" + mode, FollowerManager.Instance.GetSaveData());
            await SetItemAsync("TanningInfo:" + mode, GetTanningSave());
            await SetItemAsync("Dojos:" + mode, JsonConvert.SerializeObject(AreaManager.Instance.GetDojoSaveData()));

            LastSave = DateTime.UtcNow;
            GameState.IsSaving = false;
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to save.");
            Console.WriteLine(e.Message);
            GameState.IsSaving = false;
        }
    }
    public static async Task LoadSaveGame(string mode)
    {

        var serializerSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };

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
        
        if (await ContainsKeyAsync("Playtime:" + mode))
        {
            GameState.CurrentTick = int.Parse(Decrypt(await GetItemAsync<string>("Playtime:" + mode)));
        }
        if (await ContainsKeyAsync("LastSave:" + mode))
        {
            LastSave = DateTime.Parse(await GetItemAsync<string>("LastSave:" + mode));
        }
        if (await ContainsKeyAsync("Skills:" + mode))
        {
            string[] data = (await GetItemAsync<string>("Skills:" + mode)).Split(',');
            foreach(string d in data)
            {
                if(d.Length > 1)
                {
                    Player.Instance.Skills.Find(x => x.Name == d.Split(':')[0]).LoadExperience(long.Parse(d.Split(':')[1]));

                }
            }
        }
        if (await ContainsKeyAsync("Inventory:" + mode))
        {
            Player.Instance.Inventory.Clear();
            Player.Instance.Inventory.LoadData(await GetItemAsync<string>("Inventory:" + mode));
        }
        if (await ContainsKeyAsync("Bank:" + mode))
        {
            Bank.Instance.Inventory.Clear();
            Bank.Instance.Inventory.LoadData(await GetItemAsync<string>("Bank:" + mode));
        }
        if (await ContainsKeyAsync("Areas:" + mode))
        {
            AreaManager.Instance.LoadAreaSave(JsonConvert.DeserializeObject<List<AreaSaveData>>(await GetItemAsync<string>("Areas:" + mode)));
        }
        if (await ContainsKeyAsync("Regions:" + mode))
        {
            AreaManager.Instance.LoadRegionSave(JsonConvert.DeserializeObject<List<RegionSaveData>>(await GetItemAsync<string>("Regions:" + mode)));
        }
        if (await ContainsKeyAsync("Quests:" + mode))
        {
            QuestManager.Instance.LoadQuestSave(JsonConvert.DeserializeObject<List<QuestSaveData>>(await GetItemAsync<string>("Quests:" + mode)));
        }
        if (await ContainsKeyAsync("GameState:" + mode))
        {
            GameState.LoadSaveData(JsonConvert.DeserializeObject<GameStateSaveData>(await GetItemAsync<string>("GameState:" + mode)));
        }
        if (await ContainsKeyAsync("Followers:" + mode))
        {
            FollowerManager.Instance.LoadSaveData(await GetItemAsync<string>("Followers:" + mode));
        }
        if (await ContainsKeyAsync("Player:" + mode))
        {
            Player.Instance.LoadSaveData(JsonConvert.DeserializeObject<PlayerSaveData>(await GetItemAsync<string>("Player:" + mode)));
        }
        if (await ContainsKeyAsync("TanningInfo:" + mode))
        {
            AreaManager.Instance.LoadTanningSave(JsonConvert.DeserializeObject<List<TanningSaveData>>(await GetItemAsync<string>("TanningInfo:" + mode)));
        }
        if(await ContainsKeyAsync("Dojos:" + mode))
        {
            AreaManager.Instance.LoadDojoSaveData(JsonConvert.DeserializeObject<List<DojoSaveData>>(await GetItemAsync<string>("Dojos:" + mode)));
        }
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
    public static string GetTanningSave()
    {
        return JsonConvert.SerializeObject(AreaManager.Instance.GetTanningSaveData());
    }
    public static async Task<bool> HasSaveFile(string mode)
    {
        return await ContainsKeyAsync("Version:" + mode);
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

