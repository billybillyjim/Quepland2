using Microsoft.JSInterop;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


public static class SaveManager
{
    public static string SaveVersion = "";
    public static IJSRuntime jSRuntime;

    public static async Task SaveGame()
    {
        Stopwatch w = new Stopwatch();
        w.Start();
        await SetItemAsync("Version", GameState.Version);
        await SetItemAsync("Playtime", GetSaveString(GameState.CurrentTick));
        await SetItemAsync("Game Mode", GetSaveString(GameState.CurrentGameMode));
        w.Stop();
        Console.WriteLine("Time for version, playtime, and game mode:" + w.ElapsedMilliseconds + "ms.");
        w.Restart();
        await SetItemAsync("Skills", GetSaveString(Player.Instance.Skills)); 
        w.Stop();
        Console.WriteLine("Time for Skills:" + w.ElapsedMilliseconds + "ms.");

    }
    public static async Task LoadSaveGame()
    {
        var serializerSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };
        if(await ContainsKeyAsync("Playtime"))
        {
            GameState.CurrentTick = int.Parse(Decrypt(await GetItemAsync<string>("Playtime")));
        }
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
        if (await ContainsKeyAsync("Skills"))
        {
            Player.Instance.Skills.Clear();
            JsonConvert.PopulateObject(Decrypt(await GetItemAsync<string>("Skills")), Player.Instance.Skills, serializerSettings);
        }
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

