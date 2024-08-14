using NoMercy.Helpers;
using NoMercy.Networking;
using NoMercy.Providers.Helpers;

namespace NoMercy.Providers.FanArt.Client;

public class FanArtBaseClient : BaseClient
{
    protected override Uri BaseUrl => new("http://webservice.fanart.tv/v3/");
    protected override int ConcurrentRequests => 3;
    protected override int Interval => 1000;
    protected override Dictionary<string, string?> QueryParams { get; } = new()
    {
        { "api_key",  ApiInfo.FanArtKey }
    };

    protected FanArtBaseClient()
    {
        //
    }
}