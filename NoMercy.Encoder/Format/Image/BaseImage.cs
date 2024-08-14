using System.Text;
using FFMpegCore;
using NoMercy.Encoder.Core;
using NoMercy.Encoder.Format.Rules;
using NoMercy.NmSystem;

namespace NoMercy.Encoder.Format.Image;

public class BaseImage : Classes
{
    #region Properties

    private CodecDto ImageCodec { get; set; } = ImageCodecs.Png;
    
    protected internal VideoStream? ImageStream;

    internal List<VideoStream> ImageStreams { get; set; }
    
    private protected virtual string[] InitialParameters => [];
    private protected virtual string[] AvailableContainers => [];
    private protected virtual string[] AvailablePresets => [];
    private protected virtual string[] AvailableProfiles => [];
    private protected virtual string[] AvailableFormats => [];
    private protected virtual CodecDto[] AvailableCodecs => [];

    private readonly Dictionary<string, dynamic> _extraParameters = [];
    private readonly Dictionary<string, dynamic> _filters = [];
    private readonly Dictionary<string, dynamic> _ops = [];
    
    internal int OutputWidth { get; set; }
    internal int? OutputHeight { get; set; }
    internal int FrameRate { get; set; }
    internal string CropValue { get; set; } = "";
    internal protected CropArea Crop
    {
        get
        {
            if (string.IsNullOrEmpty(CropValue)) return new CropArea();
            var parts = CropValue.Split(':')
                .Select(int.Parse)
                .ToArray();
            return new CropArea(parts[0], parts[1], parts[2], parts[3]);
        }
        set => CropValue = $"crop={value.W}:{value.H}:{value.X}:{value.Y}";
    }
    
    internal double AspectRatioValue => Crop.H / Crop.W;

    internal string ScaleValue = "";

    public ScaleArea Scale
    {
        get
        {
            if (string.IsNullOrEmpty(ScaleValue))
                return new ScaleArea { W = 0, H = 0 };
            var scale = ScaleValue.Split(':');
            return new ScaleArea
            {
                W = scale[0].ToInt(),
                H = int.IsNegative(scale[1].ToInt()) 
                    ? Convert.ToInt32(scale[0].ToInt() * AspectRatioValue)
                    : scale[1].ToInt()
            };
        }
        set => ScaleValue = $"{value.W}:{value.H}";
    }
    
    public bool IsHdr
    {
        get => VideoIsHdr();
    }

    internal string _Filename = "";
    internal string Filename
    {
        get => _Filename
            .Replace(":framesize:", $"{Scale.W}x{Scale.H}")
            .Replace(":type:", Type);
        set => _Filename = value;
    }

    public dynamic Data => new
    {
        Container = ImageCodec.Name,
        ExtraParameters = _extraParameters,
        Filters = _filters,
        Ops = _ops,
        Type,
    };
    
    #endregion

    #region Getters

    //

    #endregion

    #region Setters

    protected BaseImage SetImageCodec(string imageCodec)
    {
        var availableCodecs = AvailableCodecs;
        if (availableCodecs.All(codec => codec.Value != imageCodec))
            throw new Exception(
                $"Wrong image codec value for {imageCodec}, available formats are {string.Join(", ", AvailableCodecs.Select(codec => codec.Value))}");

        ImageCodec = availableCodecs.First(codec => codec.Value == imageCodec);

        return this;
    }
    
    public bool VideoIsHdr()
    {
        return ImageStream?.PixelFormat.Contains("hdr") ?? false;
    }

    public BaseImage SetScale(string scale)
    {
        OutputWidth = scale.Split(":")[0].ToInt();
        ScaleValue = scale;
        return this;
    }
    
    public BaseImage SetScale(int value)
    {
        OutputWidth = value;
        ScaleValue = $"{value}:-2";
        return this;
    }

    public BaseImage SetScale(int width, int height)
    {
        OutputWidth = width;
        OutputHeight = height;
        ScaleValue = $"{width}:{height}";
        
        return this;
    }

    
    public BaseImage SetFilename(string fileName)
    {
        Filename = fileName;
        
        return this;
    }
    
    protected BaseImage  AddCustomArgument(string key, dynamic i)
    {
        _extraParameters.Add(key, i);
        return this;
    }
    
    public BaseImage AddOpts(string key, dynamic value)
    {
        _ops.Add(key, value);
        return this;
    }

    public override BaseImage ApplyFlags()
    {
        AddCustomArgument("-ss",1);
        // AddCustomArgument("-vf", $"\"fps=1/{FrameRate}\"");
        
        return this;
    }
    
    public void AddToDictionary(Dictionary<string, dynamic> commandDictionary, int index)
    {
        commandDictionary["-map"] = $"[i{index}_hls_0]";
        commandDictionary["-c:v"] = ImageCodec.Value;

        foreach (var extraParameter in _extraParameters)
        {
            commandDictionary[extraParameter.Key] = extraParameter.Value;
        }
    }

    public void CreateFolder()
    {
        var path = Path.Combine(BasePath, Filename.Split("/").First());
        // Logger.Encoder($"Creating folder {path}");
        
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }

    
    public async Task BuildSprite(ProgressMeta progressMeta)
    {
        var spriteFilename = Filename.Split("/").First() + ".webp";
        var spriteFile = Path.Combine(BasePath, spriteFilename);
        var timeFile = Path.Combine(BasePath, Filename.Split("/").First() + ".vtt");
        
        var thumbnailsFolder = Path.Combine(BasePath, Filename.Split("/").First());

        if (File.Exists(spriteFile) || !Directory.Exists(thumbnailsFolder))
        {
            return;
        }
        
        var imageFiles = Directory.GetFiles(thumbnailsFolder)
            .OrderBy(f => f)
            .ToArray();

        if (imageFiles.Length == 0)
        {
            return;
        }

        var thumbWidth = OutputWidth;
        var thumbHeight = (OutputWidth / 16) * 9;

        var gridWidth = (int)Math.Ceiling(Math.Sqrt(imageFiles.Length));
        var gridHeight = (int)Math.Ceiling((double)imageFiles.Length / gridWidth);

        var montageCommand = $"-i \"{Path.Combine(thumbnailsFolder, Filename.Split("/").First() + "-%04d.jpg")}\" -filter_complex tile=\"{gridWidth}x{gridHeight}\" -y \"{spriteFile}\"";
        
        await FfMpeg.Run(montageCommand, BasePath, progressMeta);

        var times = CreateTimeInterval(imageFiles.Length * FrameRate + 1, FrameRate);

        var dstX = 0;
        var dstY = 0;

        var jpg = 1;
        var line = 1;

        var thumbContent = new StringBuilder();
        thumbContent.AppendLine("WEBVTT");
        thumbContent.AppendLine("");

        foreach (var time in times.Take(times.Count - 1))
        {
            var index = times.IndexOf(time);
            thumbContent.AppendLine(jpg.ToString());
            thumbContent.AppendLine($"{time} --> {times[index + 1]}");
            thumbContent.AppendLine($"{spriteFilename}#xywh={dstX},{dstY},{thumbWidth},{thumbHeight}");
            thumbContent.AppendLine("");
            
            if (line > gridHeight) continue;
            
            if (jpg % gridWidth == 0)
            {
                dstX = 0;
                dstY += thumbHeight;
            }
            else
            {
                dstX += thumbWidth;
            }
            jpg++;
        }
        
        await File.WriteAllTextAsync(timeFile, thumbContent.ToString());
        
        if (Directory.Exists(thumbnailsFolder))
        {
            // Logger.Encoder($"Deleting folder {thumbnailsFolder}");
            // Directory.Delete(thumbnailsFolder, true);
        }
    }

    private List<string> CreateTimeInterval(double duration, int interval)
    {
        var d = new DateTime().Date;
        var timeArr = new List<string>();

        for (int i = 0; i <= duration / interval; i++)
        {
            var hours = d.Hour.ToString("D2");
            var minute = d.Minute.ToString("D2");
            var second = d.Second.ToString("D2");
            var miliSecond = d.Millisecond.ToString("D3");

            timeArr.Add($"{hours}:{minute}:{second}.{miliSecond}");

            d = d.AddSeconds(interval);
        }

        return timeArr;
    }
    
    public BaseImage Build()
    {
        var newStream = (BaseImage) MemberwiseClone();
        
        newStream.IsImage = true;

        newStream.ImageStream = ImageStreams.First();
        
        return newStream;
    }
    #endregion
}