using Serilog.Events;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using NoMercy.Helpers;
using NoMercy.Networking;

namespace NoMercy.Providers.Helpers;

public class BaseClient : IDisposable
{
    protected Guid Id { get; set; }

    protected readonly HttpClient Client;

    protected virtual Uri BaseUrl => new("http://localhost:8080");
    protected virtual int ConcurrentRequests  => 1;
    protected virtual int Interval => 1000;
    protected virtual Dictionary<string, string?> QueryParams => new();
    protected virtual string UserAgent => ApiInfo.UserAgent;
    private static BaseClient? _instance;

    protected BaseClient()
    {
        _instance = this;
        Client = new HttpClient()
        {
            BaseAddress = _instance.BaseUrl,
            DefaultRequestHeaders =
            {
                {"Accept", "application/json"},
                {"User-Agent", _instance.UserAgent}
            },
            Timeout = TimeSpan.FromMinutes(5)
        };
    }

    protected BaseClient(Guid id)
    {
        Id = id;
        _instance = this;
        Client = new HttpClient()
        {
            BaseAddress = _instance.BaseUrl,
            DefaultRequestHeaders =
            {
                {"Accept", "application/json"},
                {"User-Agent", _instance.UserAgent}
            },
            Timeout = TimeSpan.FromMinutes(5)
        };
    }

    private static Queue? _queue;

    protected static Queue Queue()
    {
        return _queue ??= new Queue(new QueueOptions
        {
            Concurrent = _instance?.ConcurrentRequests ?? 1, 
            Interval = _instance?.Interval ?? 1000, 
            Start = true
        });
    }

    protected virtual async Task<T?> Get<T>(string url, Dictionary<string, string?>? query, bool? priority = false)
        where T : class
    {
        query ??= new Dictionary<string, string?>();
        
        foreach (var queryParam in QueryParams)
        {
            query.Add(queryParam.Key, queryParam.Value);
        }
        
        var newUrl = QueryHelpers.AddQueryString(url, query!);

        if (CacheController.Read(newUrl, out T? result)) return result;

        Logger.Http(newUrl, LogEventLevel.Verbose);

        var response = await Queue().Enqueue(() => Client.GetStringAsync(newUrl), newUrl, priority);

        await CacheController.Write(newUrl, response);

        var data = JsonConvert.DeserializeObject<T>(response);

        return data;
    }

    public void Dispose()
    {
        Client.Dispose();
    }
}

public static partial class Url
{

    public static Uri ToHttps(this Uri url)
    {
        var uriBuilder = new UriBuilder(url)
        {
            Scheme = Uri.UriSchemeHttps,
            Port = -1 // default port for scheme
        };

        return uriBuilder.Uri;
    }
}