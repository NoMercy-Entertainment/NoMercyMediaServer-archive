using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using MimeMapping;
using NoMercy.Helpers;
using NoMercy.NmSystem;
using NoMercy.Providers.Helpers;
using NoMercy.Providers.TMDB.Client;

namespace NoMercy.Server.app.Http.Controllers.File;

[Route("images/{type}/{path}")]
public class ImageController: Controller
{
    [HttpGet]
    public async Task<IActionResult> Collections(string type, string path, [FromQuery] ImageConvertArguments request)
    {
        try
        {
            Response.Headers.Append("Expires", DateTime.Now.AddDays(30) + " GMT");
            Response.Headers.Append("Cache-Control", "public, max-age=2592000");
            Response.Headers.Append("Access-Control-Allow-Origin", "*");

            var folder = Path.Join(AppFiles.ImagesPath, type);
            if (!Directory.Exists(folder)) return NotFound();

            var filePath = Path.Join(folder, path.Replace("/", ""));
            if (!System.IO.File.Exists(filePath) && type == "original")
                await TmdbImageClient.Download("/" + path);

            if (!System.IO.File.Exists(filePath)) return NotFound();

            var fileInfo = new FileInfo(filePath);
            var originalFileSize = fileInfo.Length;
            var originalMimeType = MimeUtility.GetMimeMapping(filePath);

            var emptyArguments = request.Width is null && request.Type is null && request.Quality is 100;

            if (emptyArguments || path.Contains(".svg") ||
                (originalFileSize < request.Width && originalMimeType == request.Type.ToString()))
                return PhysicalFile(filePath, originalMimeType);

            var hashedUrl = CacheController.GenerateFileName(Request.GetEncodedUrl()) + "." +
                            (request.Type.ToString()?.ToLower() ?? fileInfo.Extension);

            var cachedImagePath = Path.Join(AppFiles.TempImagesPath, hashedUrl);
            if (System.IO.File.Exists(cachedImagePath)) return PhysicalFile(cachedImagePath, "image/" + request.Type);

            var (magickImage, mimeType) = Images.ResizeMagickNet(filePath, request);
            await System.IO.File.WriteAllBytesAsync(cachedImagePath, magickImage);

            return File(magickImage, "image/" + mimeType);
        }
        catch (Exception)
        {
            return NotFound();
        }
    }
}