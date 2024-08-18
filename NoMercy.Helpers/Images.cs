using ImageMagick;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace NoMercy.Helpers;

public class ImageConvertArguments
{
    [JsonProperty("width")] public int? Width { get; set; }
    [JsonProperty("type")] public MagickFormat? Type { get; set; }
    [JsonProperty("quality")] public int Quality { get; set; } = 100;

    [FromQuery(Name = "aspect_ratio")]
    [JsonProperty("aspect_ratio")]
    public double? AspectRatio { get; set; }
}

public static class Images
{
    public static (byte[] magickImage, string mimeType) ResizeMagickNet(string image, ImageConvertArguments arguments)
    {
        using MagickImage inputStream = ReadFileStream(image);

        double aspectRatio = arguments.AspectRatio ?? inputStream.Height / (float)inputStream.Width;

        int width = arguments.Width ?? inputStream.Width;
        int height = (int)(width * aspectRatio);

        MagickGeometry size = new(width, height)
        {
            IgnoreAspectRatio = true,
            FillArea = true
        };

        inputStream.Resize(size);
        inputStream.Strip();
        inputStream.Quality = arguments.Quality;
        inputStream.Format = arguments.Type ?? inputStream.Format;

        string mimeType = inputStream.Format.ToString();

        return (inputStream.ToByteArray(), mimeType);
    }

    private static MagickImage ReadFileStream(string image, int attempts = 0)
    {
        if (!File.Exists(image)) throw new Exception("File not found");

        while (attempts < 5)
            try
            {
                return new MagickImage(image);
            }
            catch
            {
                Thread.Sleep(100);
                return ReadFileStream(image, attempts + 1);
            }

        throw new Exception("Failed to read image");
    }
}