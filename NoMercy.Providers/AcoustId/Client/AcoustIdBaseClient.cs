using Microsoft.AspNetCore.WebUtilities;
using NoMercy.Helpers;
using NoMercy.Providers.AcoustId.Models;
using NoMercy.Providers.Helpers;
using Serilog.Events;

namespace NoMercy.Providers.AcoustId.Client;

public class AcoustIdBaseClient : BaseClient
{
    protected override Uri BaseUrl => new("https://api.acoustid.org/v2/");
    protected override int ConcurrentRequests => 2;
    protected override int Interval => 1000;

    protected override async Task<T?> Get<T>(string url, Dictionary<string, string?>? query = default, bool? priority = false)
        where T : class
    {
        query ??= new Dictionary<string, string?>();

        var newUrl = QueryHelpers.AddQueryString(url, query);

        if (CacheController.Read(newUrl, out AcoustIdFingerprint? result))
            if (result?.Results.Length > 0 && result.Results
                    .Any(fpResult => fpResult.Recordings is not null && fpResult.Recordings
                        .Any(recording => recording?.Title != null)))
                return result as T;

        Logger.AcoustId(newUrl, LogEventLevel.Verbose);

        var response = await Queue().Enqueue(() => Client.GetStringAsync(newUrl), newUrl, priority);

        await CacheController.Write(newUrl, response);

        var data = JsonHelper.FromJson<AcoustIdFingerprint>(response);

        var iteration = 0;

        if (data?.Results.Length > 0 && data.Results
                .Any(fpResult => fpResult.Recordings is not null && fpResult.Recordings
                    .Any(recording => recording?.Title != null))) return data as T;

        while (data?.Results.Length == 0 && data.Results
                   .Any(fpResult => fpResult.Recordings is not null && fpResult.Recordings
                       .Any(recording => recording?.Title == null)) && iteration < 10)
        {
            response = await Queue().Enqueue(() => Client.GetStringAsync(newUrl), newUrl, priority);

            await CacheController.Write(newUrl, response);

            Logger.Request(response, LogEventLevel.Verbose);

            data = JsonHelper.FromJson<AcoustIdFingerprint>(response);

            iteration++;
        }

        return data as T;
    }
}