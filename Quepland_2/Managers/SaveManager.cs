using Microsoft.JSInterop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
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
            await SetItemAsync("Version:" + mode, Compress(GameState.Version));
            await SetItemAsync("Playtime:" + mode, GetSaveString(GameState.CurrentTick));
            await SetItemAsync("LastSave:" + mode, DateTime.UtcNow);
            await SetItemAsync("Skills:" + mode, Compress(GetSkillsSave()));
            await SetItemAsync("Inventory:" + mode, Compress(GetItemSave(Player.Instance.Inventory)));
            await SetItemAsync("Bank:" + mode, Compress(GetItemSave(Bank.Instance.Inventory)));
            await SetItemAsync("Areas:" + mode, Compress(GetAreaSave()));
            await SetItemAsync("Regions:" + mode, Compress(GetRegionSave()));
            await SetItemAsync("Quests:" + mode, Compress(GetQuestSave()));
            await SetItemAsync("GameState:" + mode, GetSaveString(GameState.GetSaveData()));
            await SetItemAsync("Player:" + mode, GetSaveString(Player.Instance.GetSaveData()));
            await SetItemAsync("Followers:" + mode, Compress(FollowerManager.Instance.GetSaveData()));
            await SetItemAsync("TanningInfo:" + mode, Compress(GetTanningSave()));
            await SetItemAsync("Dojos:" + mode, GetSaveString(AreaManager.Instance.GetDojoSaveData()));
            await SetItemAsync("AFKAction:" + mode, GetSaveString(GameState.CurrentAFKAction));

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
            GameState.CurrentTick = int.Parse(Decompress(await GetItemAsync<string>("Playtime:" + mode)));
        }
        if (await ContainsKeyAsync("LastSave:" + mode))
        {
            LastSave = DateTime.Parse(await GetItemAsync<string>("LastSave:" + mode));
        }
        if (await ContainsKeyAsync("Skills:" + mode))
        {
            string[] data = Decompress(await GetItemAsync<string>("Skills:" + mode)).Split(',');
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
            Player.Instance.Inventory.LoadData(Decompress(await GetItemAsync<string>("Inventory:" + mode)));
        }
        if (await ContainsKeyAsync("Bank:" + mode))
        {
            Bank.Instance.Inventory.Clear();
            Bank.Instance.Inventory.LoadData(Decompress(await GetItemAsync<string>("Bank:" + mode)));
        }
        if (await ContainsKeyAsync("Areas:" + mode))
        {
            AreaManager.Instance.LoadAreaSave(JsonConvert.DeserializeObject<List<AreaSaveData>>(Decompress(await GetItemAsync<string>("Areas:" + mode))));
        }
        if (await ContainsKeyAsync("Regions:" + mode))
        {
            AreaManager.Instance.LoadRegionSave(JsonConvert.DeserializeObject<List<RegionSaveData>>(Decompress(await GetItemAsync<string>("Regions:" + mode))));
        }
        if (await ContainsKeyAsync("Quests:" + mode))
        {
            QuestManager.Instance.LoadQuestSave(JsonConvert.DeserializeObject<List<QuestSaveData>>(Decompress(await GetItemAsync<string>("Quests:" + mode))));
        }
        if (await ContainsKeyAsync("GameState:" + mode))
        {
            GameState.LoadSaveData(JsonConvert.DeserializeObject<GameStateSaveData>(Decompress(await GetItemAsync<string>("GameState:" + mode))));
        }
        if (await ContainsKeyAsync("Followers:" + mode))
        {
            FollowerManager.Instance.LoadSaveData(Decompress(await GetItemAsync<string>("Followers:" + mode)));
        }
        if (await ContainsKeyAsync("Player:" + mode))
        {
            Player.Instance.LoadSaveData(JsonConvert.DeserializeObject<PlayerSaveData>(Decompress(await GetItemAsync<string>("Player:" + mode))));
        }
        if (await ContainsKeyAsync("TanningInfo:" + mode))
        {
            AreaManager.Instance.LoadTanningSave(JsonConvert.DeserializeObject<List<TanningSaveData>>(Decompress(await GetItemAsync<string>("TanningInfo:" + mode))));
        }
        if(await ContainsKeyAsync("Dojos:" + mode))
        {
            AreaManager.Instance.LoadDojoSaveData(JsonConvert.DeserializeObject<List<DojoSaveData>>(Decompress(await GetItemAsync<string>("Dojos:" + mode))));
        }
        if(await ContainsKeyAsync("AFKAction:" + mode))
        {
            GameState.CurrentAFKAction = (JsonConvert.DeserializeObject<AFKAction>(Decompress(await GetItemAsync<string>("AFKAction:" + mode))));
        }
        Console.WriteLine(Compress("This is a test of what I can do"));
    }
    public static string GetSaveExport()
    {
        string file = "";
        //0
        file += Compress(GameState.CurrentGameMode.ToString()) + ",";
        //1
        file += Compress(GameState.Version) + ",";
        //2
        file += Compress(GameState.CurrentTick.ToString()) + ",";
        //3
        file += Compress(DateTime.UtcNow.ToString()) + ",";
        //4
        file += Compress(GetSkillsSave()) + ",";
        //5
        file += Compress(GetItemSave(Player.Instance.Inventory)) + ",";
        //6
        file += Compress(GetItemSave(Bank.Instance.Inventory)) + ",";
        //7
        file += Compress(GetAreaSave()) + ",";
        //8
        file += Compress(GetRegionSave()) + ",";
        //9
        file += Compress(GetQuestSave()) + ",";
        //10
        file += Compress(JsonConvert.SerializeObject(GameState.GetSaveData())) + ",";
        //11
        file += Compress(JsonConvert.SerializeObject(Player.Instance.GetSaveData())) + ",";
        //12
        file += Compress(FollowerManager.Instance.GetSaveData()) + ",";
        //13
        file += Compress(GetTanningSave()) + ",";
        //14
        file += Compress(JsonConvert.SerializeObject(AreaManager.Instance.GetDojoSaveData()));
        
        return file;
    }
    public static void ImportSave(string file)
    {
        string[] data = file.Split(',');

        if (Decompress(data[0]) == "Normal")
        {
            GameState.CurrentGameMode = GameState.GameType.Normal;
        }
        else if (Decompress(data[0]) == "Hardcore")
        {
            GameState.CurrentGameMode = GameState.GameType.Hardcore;
        }
        else if (Decompress(data[0]) == "Ultimate")
        {
            GameState.CurrentGameMode = GameState.GameType.Ultimate;
        }

        if (data.Length > 2)
        {
            Console.WriteLine("Data:" + data[2]);
            Console.WriteLine("Decompressed:" + Decompress(data[2]));
            GameState.CurrentTick = int.Parse(Decompress(data[2]));
        }
        if (data.Length > 3)
        {
            LastSave = DateTime.Parse(Decompress(data[3]));
        }
        if (data.Length > 4)
        {
            string[] skillData = (Decompress(data[4])).Split(',');
            foreach (string d in skillData)
            {
                if (d.Length > 1)
                {
                    Player.Instance.Skills.Find(x => x.Name == d.Split(':')[0]).LoadExperience(long.Parse(d.Split(':')[1]));

                }
            }
        }
        if (data.Length > 5)
        {
            Player.Instance.Inventory.Clear();
            Player.Instance.Inventory.LoadData(Decompress(data[5]));
        }
        if (data.Length > 6)
        {
            Bank.Instance.Inventory.Clear();
            Bank.Instance.Inventory.LoadData(Decompress(data[6]));
        }
        if (data.Length > 7)
        {
            AreaManager.Instance.LoadAreaSave(JsonConvert.DeserializeObject<List<AreaSaveData>>(Decompress(data[7])));
        }
        if (data.Length > 8)
        {
            AreaManager.Instance.LoadRegionSave(JsonConvert.DeserializeObject<List<RegionSaveData>>(Decompress(data[8])));
        }
        if (data.Length > 9)
        {
            QuestManager.Instance.LoadQuestSave(JsonConvert.DeserializeObject<List<QuestSaveData>>(Decompress(data[9])));
        }
        if (data.Length > 10)
        {
            GameState.LoadSaveData(JsonConvert.DeserializeObject<GameStateSaveData>(Decompress(data[10])));
        }
        if (data.Length > 11)
        {
            Player.Instance.LoadSaveData(JsonConvert.DeserializeObject<PlayerSaveData>(Decompress(data[11])));           
        }
        if (data.Length > 12)
        {
            FollowerManager.Instance.LoadSaveData(Decompress(data[12]));
        }
        if (data.Length > 13)
        {
            AreaManager.Instance.LoadTanningSave(JsonConvert.DeserializeObject<List<TanningSaveData>>(Decompress(data[13])));
        }
        if (data.Length > 14)
        {
            AreaManager.Instance.LoadDojoSaveData(JsonConvert.DeserializeObject<List<DojoSaveData>>(Decompress(data[14])));
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
        return Compress(JsonConvert.SerializeObject(o));
    }
    public static string Compress(string s)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(s);
        using (var stream = new MemoryStream())
        {
            var len = BitConverter.GetBytes(bytes.Length);
            stream.Write(len, 0, 4);
            using (var compressionStream = new GZipStream(stream, CompressionMode.Compress))
            {
                
                compressionStream.Write(bytes, 0, bytes.Length);
            }
            return Convert.ToBase64String(stream.ToArray());
        }
    }
    public static string Decompress(string s)
    {
        using (var source = new MemoryStream(Convert.FromBase64String(s)))
        {
            byte[] len = new byte[4];
            source.Read(len, 0, 4);
            var l = BitConverter.ToInt32(len, 0);
            using(var decompressionStream = new GZipStream(source, CompressionMode.Decompress))
            {
                var result = new byte[l];
                decompressionStream.Read(result, 0, l);
                return Encoding.UTF8.GetString(result);
            }
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

