using Microsoft.AspNetCore.WebUtilities;
using NoMercy.Helpers;
using NoMercy.Networking;
using NoMercy.Providers.Helpers;
using Serilog.Events;

namespace NoMercy.Providers.MusixMatch.Client;

public class MusixMatchBaseClient : BaseClient
{
    protected override Uri BaseUrl => new("https://apic-desktop.musixmatch.com/ws/1.1/");
    protected override int ConcurrentRequests => 2;
    protected override int Interval => 1000;

    protected MusixMatchBaseClient()
    {
        Client.DefaultRequestHeaders.Add("authority", "apic-desktop.musixmatch.com");
        Client.DefaultRequestHeaders.Add("cookie", "x-mxm-token-guid=");
    }

    protected override async Task<T?> Get<T>(string url, Dictionary<string, string?>? query, bool? priority = false)
        where T : class
    {        
        query ??= new Dictionary<string, string?>();
        query.Add("format", "json");
        query.Add("namespace", "lyrics_richsynched");
        query.Add("subtitle_format", "mxm");
        query.Add("app_id", "web-desktop-app-v1.0");
        query.Add("usertoken", ApiInfo.MusixmatchKey);

        var newUrl = QueryHelpers.AddQueryString(url, query!);

        if (CacheController.Read(newUrl, out T? result)) return result;

        Logger.MusixMatch(newUrl, LogEventLevel.Verbose);

        var response = await Queue().Enqueue(() => Client.GetStringAsync(newUrl), newUrl, priority);

        await CacheController.Write(newUrl, response);

        var data = JsonHelper.FromJson<T>(response);

        return data;
    }
}