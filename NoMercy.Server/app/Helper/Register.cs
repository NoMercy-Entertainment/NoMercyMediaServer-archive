using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NoMercy.Database;
using NoMercy.Database.Models;
using NoMercy.Helpers;
using NoMercy.Networking;

namespace NoMercy.Server.app.Helper;

public static class Register
{
    private static string DeviceName()
    {
        MediaContext mediaContext = new();
        var device = mediaContext.Configuration.FirstOrDefault(device => device.Key == "server_name");
        return device?.Value ?? Environment.MachineName;
    }

    public static Task Init()
    {
        Dictionary<string, string> serverData = new()
        {
            { "server_id", NmSystem.Info.DeviceId.ToString() },
            { "server_name", DeviceName() },
            { "internal_ip", Networking.Networking.InternalIp },
            { "internal_port", Config.InternalServerPort.ToString() },
            { "external_port", Config.ExternalServerPort.ToString() },
            { "server_version", ApiInfo.ApplicationVersion },
            { "platform", NmSystem.Info.Platform }
        };

        Logger.Register(@"Registering Server, this takes a moment...");

        HttpClient client = new();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var content = client.PostAsync("https://api-dev.nomercy.tv/v1/server/register",
                new FormUrlEncodedContent(serverData))
            .Result.Content.ReadAsStringAsync().Result;

        var data = JsonConvert.DeserializeObject(content);
        
        // Logger.Register(data);

        if (data == null) throw new Exception("Failed to register Server");

        Logger.Register(@"Server registered successfully");

        AssignServer().Wait();

        return Task.CompletedTask;
    }

    private static Task AssignServer()
    {
        Dictionary<string, string> serverData = new()
        {
            { "server_id", NmSystem.Info.DeviceId.ToString() }
        };

        HttpClient client = new();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Auth.AccessToken);

        var content = client
            .PostAsync("https://api-dev.nomercy.tv/v1/server/assign", new FormUrlEncodedContent(serverData))
            .Result.Content.ReadAsStringAsync().Result;

        var data = JsonConvert.DeserializeObject<ServerRegisterResponse>(content);

        if (data == null) throw new Exception("Failed to assign Server");
        
        var newUser = new User
        {
            Id = data.User.Id,
            Name = data.User.Name,
            Email = data.User.Email,
            Owner = true,
            Allowed = true,
            Manage = true,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            AudioTranscoding = true,
            NoTranscoding = true,
            VideoTranscoding = true,
        };
        
        using MediaContext mediaContext = new();
        mediaContext.Users.Upsert(newUser)
            .On(x => x.Id)
            .Run();
        
        ClaimsPrincipleExtensions.AddUser(newUser);

        Logger.Register(@"Server assigned successfully");

        Certificate.RenewSslCertificate().Wait();

        return Task.CompletedTask;
    }
}

public class ServerRegisterResponse
{
    [JsonProperty("status")] public string Status { get; set; }
    [JsonProperty("id")] public string ServerId { get; set; }
    [JsonProperty("user")] public User User { get; set; }
}