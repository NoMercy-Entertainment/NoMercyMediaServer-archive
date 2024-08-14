namespace NoMercy.NmSystem;

public static class Url
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
    
    public static string FileName(this Uri url)
    {
        return Path.GetFileName(url.LocalPath);
    }
    
    public static string BasePath(this Uri url)
    {
        return url.ToString().Replace("/" + url.FileName(), "");
    }

    public static bool HasSuccessStatus(this Uri url, string? contentType = null)
    { 
        try
        {
            HttpClient httpClient = new();
            httpClient.DefaultRequestHeaders.Add("User-Agent",  "NoMercy wMediaServer/0.1.0 ( admin@nomercy.tv )");
            
            if (contentType is not null)
                httpClient.DefaultRequestHeaders.Add("Accept", contentType);
                
            var res = httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, url)).Result;
            return res.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }
}