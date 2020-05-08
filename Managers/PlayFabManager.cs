using Microsoft.JSInterop;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


public static class PlayFabManager
{
    public static string ID = "";
    public static string KongregateID = "";
    public static string KongregateUsername = "";
    public static string KongregateToken = "";
    public static string CloudSaveVersion = "None";
    public static DateTime DateLastSave;
    public static bool IsConnected;
    public static IJSRuntime jSRuntime;
    public static IJSInProcessRuntime jSInProcessRuntime;
    public async static Task Connect()
    {
        await ConnectToKongregate();
        PlayFabSettings.staticSettings.TitleId = "11BAD";
        if (KongregateID != "" && KongregateID != null && KongregateToken != "" && KongregateToken != null)
        {
            var request = new LoginWithKongregateRequest { KongregateId = KongregateID, AuthTicket = KongregateToken, CreateAccount = true };
            var loginTask = await PlayFabClientAPI.LoginWithKongregateAsync(request);
            OnLoginComplete(loginTask);
        }
        else
        {
            var request = new LoginWithCustomIDRequest { CustomId = GameState.Guid.ToString(), CreateAccount = true };
            var loginTask = await PlayFabClientAPI.LoginWithCustomIDAsync(request);
            OnLoginComplete(loginTask);
        }

    }
    public static async Task ConnectAccountToKongregate()
    {
        if (IsConnected == false)
        {
            await Connect();
        }
        else
        {
            await ConnectToKongregate();
        }
        if (KongregateID != "" && KongregateID != null)
        {
            LinkKongregateAccountRequest req = new LinkKongregateAccountRequest { AuthTicket = KongregateToken, KongregateId = KongregateID };
            var linkTask = await PlayFabClientAPI.LinkKongregateAsync(req);
            OnLinkComplete(linkTask);
        }
    }
    private static void OnLinkComplete(PlayFabResult<LinkKongregateAccountResult> result)
    {
        var error = result.Error;
        var res = result.Result;

        if (error != null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Failed to link accounts.");
            Console.WriteLine(PlayFabUtil.GenerateErrorReport(error));
            Console.ForegroundColor = ConsoleColor.Gray;
        }
        else if (res != null)
        {
            Console.Write("Successfully connected accounts.");
        }
    }
    private static void OnLoginComplete(PlayFabResult<LoginResult> taskResult)
    {
        var apiError = taskResult.Error;
        var apiResult = taskResult.Result;

        if (apiError != null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Failed to login.");
            Console.WriteLine(PlayFabUtil.GenerateErrorReport(apiError));
            Console.ForegroundColor = ConsoleColor.Gray;
        }
        else if (apiResult != null)
        {
            ID = apiResult.PlayFabId;

            IsConnected = true;
        }

    }
    public async static Task SaveGame(string data)
    {
        var time = await PlayFabClientAPI.GetTimeAsync(new GetTimeRequest());
        DateTime t = new DateTime();
        if (time.Error == null)
        {
            t = time.Result.Time;
        }
        else if (time.Error.Error == PlayFabErrorCode.NotAuthenticated)
        {
            IsConnected = false;
            await Connect();
        }
        await PlayFabClientAPI.UpdateUserDataAsync(new UpdateUserDataRequest()
        {
            Data = new Dictionary<string, string>()
            {
                { "TestData", data }
            }
        });

    }
    public async static Task LoadGameFromCloud()
    {
        var serializerSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };
        var result = (await PlayFabClientAPI.GetUserDataAsync(new GetUserDataRequest()));
        if (result.Error != null)
        {

        }
        else
        {
            var data = result.Result;
            Console.WriteLine("getting save data...");
            Console.WriteLine("Save:" + data.Data["TestData"].Value);
        }
    }
    public async static Task CheckForCloudSave()
    {
        if (IsConnected == false)
        {
            return;
        }
        var result = (await PlayFabClientAPI.GetUserDataAsync(new GetUserDataRequest()));
        if (result.Error != null)
        {

        }
        else
        {
            var data = result.Result;
            if (data.Data.ContainsKey("Version"))
            {
                string saveVersion = data.Data["Version"].Value;
                if (saveVersion != null && saveVersion != "")
                {
                    CloudSaveVersion = saveVersion;
                }
                if (data.Data.TryGetValue("Date of Save", out _))
                {
                    if (DateTime.TryParse(data.Data["Date of Save"].Value, out DateTime t))
                    {
                        DateLastSave = t;
                    }
                }
            }
        }
    }
    public async static Task ConnectToKongregate()
    {
        try
        {
            KongregateID = (await jSRuntime.InvokeAsync<int>("kongregateFunctions.getUserID")).ToString();
        }
        catch
        {
            Console.WriteLine("Failed to get UserID");
        }
        try
        {
            KongregateToken = await jSRuntime.InvokeAsync<string>("kongregateFunctions.getToken");
        }
        catch
        {
            Console.WriteLine("Failed to get token");
        }
        try
        {
            KongregateUsername = await jSRuntime.InvokeAsync<string>("kongregateFunctions.getUsername");
        }
        catch
        {
            Console.WriteLine("Failed to get username");
        }

    }
}

