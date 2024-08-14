using NoMercy.Helpers;
using NoMercy.Networking;
using NoMercy.Providers.Helpers;

namespace NoMercy.Providers.TADB.Client;

public class TadbBaseClient : BaseClient
{
    protected override Uri BaseUrl => new($"https://www.theaudiodb.com/api/v1/json/{ApiInfo.TadbKey}/");
    protected override int ConcurrentRequests => 2;
    protected override int Interval => 1000;
    
    protected TadbBaseClient()
    {
        //
    }
}