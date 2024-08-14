using System.Security.Cryptography;
using System.Text;
using NoMercy.Helpers;
using NoMercy.NmSystem;

namespace NoMercy.Providers.Helpers;

public static class CacheController
{
    public static string GenerateFileName(string url)
    {
        return CreateMd5(url);
    }

    private static string CreateMd5(string input)
    {
        var inputBytes = Encoding.ASCII.GetBytes(input);
        var hashBytes = MD5.HashData(inputBytes);

        return Convert.ToHexString(hashBytes);
    }

    public static bool Read<T>(string url, out T? value) where T : class?
    {
        var fullname = Path.Combine(AppFiles.ApiCachePath, GenerateFileName(url));
        lock (fullname)
        {
            if (File.Exists(fullname) == false)
            {
                value = default;
                return false;
            }

            T? data;
            try
            {
                var d = File.ReadAllTextAsync(fullname).Result;
                data = JsonHelper.FromJson<T>(d);
            }
            catch (Exception)
            {
                value = default;
                return false;
            }

            if (data == null)
            {
                value = default;
                return true;
            }

            if (data is { } item)
            {
                value = item;
                return true;
            }

            value = default;
            return false;
        }
    }

    public static async Task Write(string url, string data)
    {
        var fullname = Path.Combine(AppFiles.ApiCachePath, GenerateFileName(url));

        try
        {
            await File.WriteAllTextAsync(fullname, data);
        }
        catch (Exception)
        {
            Logger.App($"CacheController: Failed to write {fullname}");
            await Write(url, data);
        }
    }
}