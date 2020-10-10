using Ionic.Zip;
using Ionic.Zlib;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
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
            if(GameState.ShowStartMenu || GameState.InitCompleted == false)
            {
                return;
            }
            GameState.UseNewSaveCompression = true;
            string mode = GameState.CurrentGameMode.ToString();
            await SetItemAsync("Version:" + mode, Compress(GameState.Version));
            await SetItemAsync("Playtime:" + mode, GetSaveString(GameState.CurrentTick));
            await SetItemAsync("LastSave:" + mode, DateTime.UtcNow.ToString(System.Globalization.CultureInfo.InvariantCulture));
            await SetItemAsync("Skills:" + mode, Compress(GetSkillsSave()));
            await SetItemAsync("Inventory:" + mode, Compress(GetItemSave(Player.Instance.Inventory)));
            await SetItemAsync("Bank:" + mode, Compress(GetItemSave(Bank.Instance.Inventory)));
            await SetItemAsync("BankTabs:" + mode, GetSaveString(Bank.Instance.Tabs));
            await SetItemAsync("Areas:" + mode, Compress(GetAreaSave()));
            await SetItemAsync("Regions:" + mode, Compress(GetRegionSave()));
            await SetItemAsync("Dungeons:" + mode, GetSaveString(AreaManager.Instance.GetDungeonSaveData()));
            await SetItemAsync("Quests:" + mode, Compress(GetQuestSave()));
            await SetItemAsync("GameState:" + mode, GetSaveString(GameState.GetSaveData()));
            await SetItemAsync("Player:" + mode, GetSaveString(Player.Instance.GetSaveData()));
            await SetItemAsync("Followers:" + mode, Compress(FollowerManager.Instance.GetSaveData()));
            await SetItemAsync("TanningInfo:" + mode, Compress(GetTanningSave()));
            await SetItemAsync("Dojos:" + mode, GetSaveString(AreaManager.Instance.GetDojoSaveData()));
            await SetItemAsync("AFKAction:" + mode, GetSaveString(GameState.CurrentAFKAction));
            await SetItemAsync("Tomes:" + mode, GetSaveString(ItemManager.Instance.Tomes));
            await SetItemAsync("NewSaveCompression", "true");
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
        int errorcode = 0;
        try
        {
            if (await ContainsKeyAsync("NewSaveCompression"))
            {
                GameState.UseNewSaveCompression = true;
            }
            if (mode == "Normal")
            {
                GameState.CurrentGameMode = GameState.GameType.Normal;
            }
            else if (mode == "Hardcore")
            {
                GameState.CurrentGameMode = GameState.GameType.Hardcore;
            }
            else if (mode == "Ultimate")
            {
                GameState.CurrentGameMode = GameState.GameType.Ultimate;
            }

            if (await ContainsKeyAsync("Playtime:" + mode))
            {
                GameState.CurrentTick = int.Parse(Decompress(await GetItemAsync<string>("Playtime:" + mode)));
            }
            errorcode++; //1
            if (await ContainsKeyAsync("LastSave:" + mode))
            {
                LastSave = DateTime.Parse(await GetItemAsync<string>("LastSave:" + mode), System.Globalization.CultureInfo.InvariantCulture);
            }
            errorcode++; //2
            if (await ContainsKeyAsync("Skills:" + mode))
            {
                string[] data = Decompress(await GetItemAsync<string>("Skills:" + mode)).Split(',');
                foreach (string d in data)
                {
                    if (d.Length > 1)
                    {
                        Player.Instance.Skills.Find(x => x.Name == d.Split(':')[0]).LoadExperience(long.Parse(d.Split(':')[1]));

                    }
                }
            }
            errorcode++; //3
            if (await ContainsKeyAsync("Inventory:" + mode))
            {
                Player.Instance.Inventory.Clear();
                Player.Instance.Inventory.LoadData(Decompress(await GetItemAsync<string>("Inventory:" + mode)));
            }
            errorcode++; //4
            if (await ContainsKeyAsync("Bank:" + mode))
            {
                Bank.Instance.Inventory.Clear();
                Bank.Instance.Inventory.LoadData(Decompress(await GetItemAsync<string>("Bank:" + mode)));
            }
            errorcode++; //5
            if (await ContainsKeyAsync("BankTabs:" + mode))
            {
                Bank.Instance.LoadTabs(DeserializeToList(Decompress(await GetItemAsync<string>("BankTabs:" + mode))));
            }
            errorcode++; //6
            if (await ContainsKeyAsync("Areas:" + mode))
            {
                AreaManager.Instance.LoadAreaSave(JsonConvert.DeserializeObject<List<AreaSaveData>>(Decompress(await GetItemAsync<string>("Areas:" + mode))));
            }
            errorcode++; //7
            if (await ContainsKeyAsync("Regions:" + mode))
            {
                AreaManager.Instance.LoadRegionSave(JsonConvert.DeserializeObject<List<RegionSaveData>>(Decompress(await GetItemAsync<string>("Regions:" + mode))));
            }
            errorcode++; //8
            if (await ContainsKeyAsync("Dungeons:" + mode))
            {
                AreaManager.Instance.LoadDungeonSaveData(JsonConvert.DeserializeObject<List<DungeonSaveData>>(Decompress(await GetItemAsync<string>("Dungeons:" + mode))));
            }
            errorcode++; //9
            if (await ContainsKeyAsync("Quests:" + mode))
            {
                QuestManager.Instance.LoadQuestSave(JsonConvert.DeserializeObject<List<QuestSaveData>>(Decompress(await GetItemAsync<string>("Quests:" + mode))));
            }
            errorcode++; //10
            if (await ContainsKeyAsync("GameState:" + mode))
            {
                GameState.LoadSaveData(JsonConvert.DeserializeObject<GameStateSaveData>(Decompress(await GetItemAsync<string>("GameState:" + mode))));
            }
            errorcode++; //11
            if (await ContainsKeyAsync("Followers:" + mode))
            {
                FollowerManager.Instance.LoadSaveData(Decompress2(await GetItemAsync<string>("Followers:" + mode)));
            }
            errorcode++; //12
            if (await ContainsKeyAsync("Player:" + mode))
            {
                Player.Instance.LoadSaveData(JsonConvert.DeserializeObject<PlayerSaveData>(Decompress(await GetItemAsync<string>("Player:" + mode))));
            }
            errorcode++; //13
            if (await ContainsKeyAsync("TanningInfo:" + mode))
            {
                AreaManager.Instance.LoadTanningSave(JsonConvert.DeserializeObject<List<TanningSaveData>>(Decompress(await GetItemAsync<string>("TanningInfo:" + mode))));
            }
            errorcode++; //14
            if (await ContainsKeyAsync("Dojos:" + mode))
            {
                AreaManager.Instance.LoadDojoSaveData(JsonConvert.DeserializeObject<List<DojoSaveData>>(Decompress(await GetItemAsync<string>("Dojos:" + mode))));
            }
            errorcode++; //15
            if (await ContainsKeyAsync("AFKAction:" + mode))
            {
                GameState.LoadAFKActionData(JsonConvert.DeserializeObject<AFKAction>(Decompress(await GetItemAsync<string>("AFKAction:" + mode))));
            }
            errorcode++; //16
            if (await ContainsKeyAsync("Tomes:" + mode))
            {
                ItemManager.Instance.Tomes = JsonConvert.DeserializeObject<List<TomeData>>(Decompress(await GetItemAsync<string>("Tomes:" + mode)));
            }
            errorcode++; //17
        }
        catch(Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);
            MessageManager.AddMessage("Failed to load save. Error code:" + errorcode);
        }
        
        //Console.WriteLine(Compress("This is a test of what I can do"));
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
        file += Compress(DateTime.UtcNow.ToString(System.Globalization.CultureInfo.InvariantCulture)) + ",";
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
        file += Compress(JsonConvert.SerializeObject(AreaManager.Instance.GetDojoSaveData())) + ",";
        //15
        file += Compress(JsonConvert.SerializeObject(Bank.Instance.Tabs)) + ",";
        //16
        file += GetSaveString(AreaManager.Instance.GetDungeonSaveData()) + ",";
        //17
        file += GetSaveString(ItemManager.Instance.Tomes);

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
        if(data.Length > 1)
        {
            Console.WriteLine(Decompress(data[1]));
        }
        if (data.Length > 2)
        {
            GameState.CurrentTick = int.Parse(Decompress(data[2]));
        }
        if (data.Length > 3)
        {
            try
            {
                LastSave = DateTime.Parse(Decompress(data[3]));
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                LastSave = DateTime.MinValue;
            }
            
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
            //Console.WriteLine(Decompress(data[6]));
            Bank.Instance.Inventory.Clear();
            Bank.Instance.Inventory.FixItems = true;
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
        if (data.Length > 15)
        {
            Bank.Instance.LoadTabs(DeserializeToList(Decompress(data[15])));
        }
        if(data.Length > 16)
        {
            AreaManager.Instance.LoadDungeonSaveData(JsonConvert.DeserializeObject<List<DungeonSaveData>>(Decompress(data[16])));
        }
        if(data.Length > 17)
        {
            ItemManager.Instance.Tomes = JsonConvert.DeserializeObject<List<TomeData>>(Decompress(data[17]));
        }
    }
    public static string GetItemSave(Inventory i)
    { 
        string data = "";
        foreach(KeyValuePair<GameItem, int> pair in i.GetItems())
        {
            data += pair.Key.UniqueID + "_" + pair.Value + "_" + JsonConvert.SerializeObject(pair.Key.Tabs) +  "_" + pair.Key.IsLocked + "/";
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
    public static string GetFollowerSkillSave(Follower f)
    {
        string s = "";
        s += f.Name + ":" + f.Banking.Experience + ",";
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
    public static List<string> DeserializeToList(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return new List<string>();
        }
        return JsonConvert.DeserializeObject<List<string>>(s);
    }
    public static string Compress(string s)
    {
        if (GameState.UseNewSaveCompression)
        {
            return Compress2(s);
        }
        byte[] bytes = Encoding.UTF8.GetBytes(s);
        using (var stream = new MemoryStream())
        {
            var len = BitConverter.GetBytes(bytes.Length);
            stream.Write(len, 0, 4);
            using (var compressionStream = new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Compress))
            {               
                compressionStream.Write(bytes, 0, bytes.Length);
            }
            return Convert.ToBase64String(stream.ToArray());
        }
    }
    public static string Compress2(string s)
    {
        return Convert.ToBase64String(Ionic.Zlib.GZipStream.CompressString(s));
    }
    public static string Decompress2(string s)
    {
        return Ionic.Zlib.GZipStream.UncompressString(Convert.FromBase64String(s));
    }
    public static string Decompress(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return "";
        }
        if (GameState.UseNewSaveCompression)
        {
            return Decompress2(s);
        }
        try
        {
            Span<byte> buffer = new Span<byte>(new byte[s.Length]);
            if (Convert.TryFromBase64String(s, buffer, out int b) == false)
            {
                if (Convert.TryFromBase64String(s.PadRight(s.Length / 4 * 4 + (s.Length % 4 == 0 ? 0 : 4), '='), new Span<byte>(new byte[s.Length]), out int pad))
                {
                    Console.WriteLine("String Length:" + s.Length);
                    Console.WriteLine("Bytes parsed:" + pad);
                    Console.WriteLine("End padding:" + s.Substring(s.Length - 10));
                    using (var source = new MemoryStream(Convert.FromBase64String(s)))
                    {
                        byte[] len = new byte[4];
                        source.Read(len, 0, 4);
                        var l = BitConverter.ToInt32(len, 0);
                        using (var decompressionStream = new System.IO.Compression.GZipStream(source, System.IO.Compression.CompressionMode.Decompress))
                        {
                            var result = new byte[l];
                            decompressionStream.Read(result, 0, l);
                            return Encoding.UTF8.GetString(result);
                        }
                    }

                }
                else
                {
                    Console.WriteLine("Padding failed.");
                    Console.WriteLine("String Length:" + s.Length);
                    Console.WriteLine("Bytes parsed:" + b);
                    Console.WriteLine("End padding:" + s.Substring(s.Length - 10));

                }

            }
            using (var source = new MemoryStream(Convert.FromBase64String(s)))
            {
                byte[] len = new byte[4];
                source.Read(len, 0, 4);
                var l = BitConverter.ToInt32(len, 0);
                using (var decompressionStream = new System.IO.Compression.GZipStream(source, System.IO.Compression.CompressionMode.Decompress))
                {
                    var result = new byte[l];
                    decompressionStream.Read(result, 0, l);
                    return Encoding.UTF8.GetString(result);
                }
            }
        }
        
    catch{
            return Decompress2(s);
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

