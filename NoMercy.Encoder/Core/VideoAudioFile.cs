using System.Text;
using System.Text.RegularExpressions;
using NoMercy.Encoder.Format.Audio;
using NoMercy.Encoder.Format.Container;
using NoMercy.Encoder.Format.Image;
using NoMercy.Encoder.Format.Rules;
using NoMercy.Encoder.Format.Subtitle;
using NoMercy.Encoder.Format.Video;

namespace NoMercy.Encoder.Core;

public class VideoAudioFile(MediaAnalysis fMediaAnalysis, string ffmpegPath): Classes
{
    public string FfmpegPath => ffmpegPath;
    
    public VideoAudioFile AddContainer(BaseContainer container)
    {
        var cropValue = CropDetect(fMediaAnalysis.Path);
            
        Container = container;
        Container.Title = Title;
        Container.InputFile = fMediaAnalysis.Path;
        Container.BasePath = BasePath;
        Container.FileName = FileName;
        Container.IsImage = IsImage;
        Container.IsAudio = IsAudio;
        Container.IsVideo = IsVideo;
        Container.IsSubtitle = IsSubtitle;
        Container.MediaAnalysis = fMediaAnalysis!;
        Container.ApplyFlags();
        
        foreach (var keyValuePair in Container.Streams)
        {
            keyValuePair.Value.BasePath = Container.BasePath;
            keyValuePair.Value.FileName = Container.FileName;

            if (keyValuePair.Value.IsVideo)
            {
                (keyValuePair.Value as BaseVideo)!.CropValue = cropValue;
                
                (keyValuePair.Value as BaseVideo)!.VideoStreams = [fMediaAnalysis!.PrimaryVideoStream!];
                (keyValuePair.Value as BaseVideo)!.VideoStream = fMediaAnalysis!.PrimaryVideoStream!;
                (keyValuePair.Value as BaseVideo)!.Index = fMediaAnalysis!.PrimaryVideoStream!.Index;
                (keyValuePair.Value as BaseVideo)!.Title = Title;

                Container.VideoStreams.Add((keyValuePair.Value as BaseVideo)!.Build());
            
            }
            else if (keyValuePair.Value.IsAudio)
            {
                (keyValuePair.Value as BaseAudio)!.AudioStreams = fMediaAnalysis!.AudioStreams!;
                (keyValuePair.Value as BaseAudio)!.AudioStream = fMediaAnalysis!.PrimaryAudioStream!;

                Container.AudioStreams.AddRange((keyValuePair.Value as BaseAudio)!.Build());
                
            }
            else if (keyValuePair.Value.IsSubtitle)
            {
                (keyValuePair.Value as BaseSubtitle)!.SubtitleStreams = fMediaAnalysis!.SubtitleStreams!;
                (keyValuePair.Value as BaseSubtitle)!.SubtitleStream = fMediaAnalysis!.PrimarySubtitleStream!;

                Container.SubtitleStreams.AddRange((keyValuePair.Value as BaseSubtitle)!.Build());
            }
            else if (keyValuePair.Value.IsImage)
            {
                (keyValuePair.Value as BaseImage)!.CropValue = cropValue;
                
                (keyValuePair.Value as BaseImage)!.ImageStreams = [fMediaAnalysis!.PrimaryVideoStream!];
                (keyValuePair.Value as BaseImage)!.ImageStream = fMediaAnalysis!.PrimaryVideoStream!;
                
                Container.ImageStreams.Add((keyValuePair.Value as BaseImage)!.Build());
            }
            
            (keyValuePair.Value as Classes)!.ApplyFlags();
        }

        return this;
    }

    public VideoAudioFile SetBasePath(string basePath)
    {
        BasePath = basePath;
        return this;
    }
    
    public VideoAudioFile SetTitle(string title)
    {
        Title = title;
        return this;
    }

    public VideoAudioFile ToFile(string filename)
    {
        FileName = filename;
        return this;
    }
    
    private string ChooseCrop(Dictionary<string, int> crops)
    {
        var maxKey = "";
        var maxValue = 0;

        foreach (KeyValuePair<string, int> crop in crops)
        {
            if (crop.Value > maxValue)
            {
                maxValue = crop.Value;
                maxKey = crop.Key;
            }
        }
        
        return maxKey;
    }

    private string CropDetect(string path)
    {
        const int sections = 10;
        
        var duration = fMediaAnalysis.Duration.TotalSeconds;
        var max = Math.Floor(duration / 2);
        var step = Math.Floor(max / sections);
        
        var counts = new Dictionary<string, int>();
        var regex = new Regex(@"crop=(\d+:\d+:\d+:\d+)", RegexOptions.Multiline);
        
        var promises = new List<Task<string>>();

        for (var i = 0; i < sections; i++) {
            var execString = $"-threads 1 -nostats -hide_banner -probesize 1024M -analyzeduration 99M -ss {i * step} -i \"{path}\" -max_muxing_queue_size 99 -vframes 2 -vf cropdetect -t {1} -f null -";
            
            var result = FfMpeg.Exec(args: execString, executable: FfmpegPath);
            promises.Add(result);
        }
        
        var results = Task.WhenAll(promises).Result;
        
        foreach (var output in results)
        {
            var matches = regex.Matches(output);
            
            foreach (Match match in matches)
            {
                var crop = match.Groups[1].Value;
                if (!counts.TryAdd(crop, 1))
                {
                    counts[crop]++;
                }
            }
        }
        
        return ChooseCrop(counts);
    }

    public void Build()
    {
        
    }
    
    public string GetFullCommand()
    {
        int threadCount = Environment.ProcessorCount;

        var command = new StringBuilder();
        
        command.Append(" -hide_banner -probesize 4092M -analyzeduration 9999M");
        command.Append($" -threads {Math.Floor(threadCount * 0.7)} ");

        if (HasGpu)
        {
            command.Append(" -extra_hw_frames 3 -init_hw_device opencl=ocl -progress - ");
        }
            
        command.Append($" -y -i \"{Container.InputFile}\" ");
        
        command.Append(" -max_muxing_queue_size 9999 ");
        
        command.Append(" -filter_complex \"");

        var isHdr = false;
        
        var complexString = new StringBuilder();
        foreach (var stream in Container.VideoStreams)
        {
            var index = Container!.VideoStreams.IndexOf(stream);
            
            if (stream.ConverToSdr && stream.IsHdr)
            {
                isHdr = stream.IsHdr;
                complexString.Append($"[v:{stream.Index}]crop={stream.CropValue},scale={stream.ScaleValue},zscale=tin=smpte2084:min=bt2020nc:pin=bt2020:rin=tv:t=smpte2084:m=bt2020nc:p=bt2020:r=tv,zscale=t=linear:npl=100,format=gbrpf32le,zscale=p=bt709,tonemap=tonemap=hable:desat=0,zscale=t=bt709:m=bt709:r=tv,format={stream.PixelFormat}[v{index}_hls_0]");
            }
            else
            {
                complexString.Append(
                    $"[{stream.Index}]crop={stream.CropValue},scale={stream.ScaleValue},format={stream.PixelFormat}[v{index}_hls_0]");
            }

            if (index != Container.VideoStreams.Count - 1)
            {
                complexString.Append(';');
            }
        }

        if (Container.AudioStreams.Count > 0)
        {
            complexString.Append(';');
        }
        foreach (var stream in Container.AudioStreams)
        {
            var index = Container!.AudioStreams.IndexOf(stream);
            
            complexString.Append($"[a:{stream.Index}]volume=3,loudnorm[a{index}_hls_0]");
            
            if (index != Container.AudioStreams.Count - 1)
            {
                complexString.Append(';');
            }
        }
        
        if (Container.SubtitleStreams.Count > 0)
        {
            complexString.Append(';');
        }
        foreach (var stream in Container.SubtitleStreams)
        {
            var index = Container!.SubtitleStreams.IndexOf(stream);
            
            complexString.Append($"[s:{stream.Index}]overlay[s{index}_hls_0]");
            
            if (index != Container.SubtitleStreams.Count - 1)
            {
                complexString.Append(';');
            }
        }
        
        if(Container.ImageStreams.Count > 0)
        {
            complexString.Append(';');
        }
        foreach (var stream in Container.ImageStreams)
        {
            var index = Container!.ImageStreams.IndexOf(stream);
            
            if (isHdr)
            {
                complexString.Append($"[v:{stream.Index}]crop={stream.CropValue},scale={stream.ScaleValue},zscale=tin=smpte2084:min=bt2020nc:pin=bt2020:rin=tv:t=smpte2084:m=bt2020nc:p=bt2020:r=tv,zscale=t=linear:npl=100,format=gbrpf32le,zscale=p=bt709,tonemap=tonemap=hable:desat=0,zscale=t=bt709:m=bt709:r=tv,fps=1/{stream.FrameRate}[i{index}_hls_0]");
            }
            else
            {
                complexString.Append(
                    $"[v:{stream.Index}]crop={stream.CropValue},scale={stream.ScaleValue},fps=1/{stream.FrameRate}[i{index}_hls_0]");
            }

            if (index != Container.ImageStreams.Count - 1)
            {
                complexString.Append(';');
            }
        }
        
        command.Append(complexString + "\"");

        foreach (var stream in Container.VideoStreams)
        {
            var commandDictionary = new Dictionary<string, dynamic>();
            
            var index = Container!.VideoStreams.IndexOf(stream);
            
            stream.AddToDictionary(commandDictionary, index);
            
            foreach (var parameter in Container?._extraParameters ?? new Dictionary<string, dynamic>())
            {
                commandDictionary[parameter.Key] = parameter.Value;
            }
            
            // commandDictionary["-t"] = 300;
            
            if (Container!.ContainerDto.Name == VideoContainers.Hls)
            {
                commandDictionary["-hls_segment_filename"] = $"\"./{stream.HlsPlaylistFilename}_%05d.ts\"";
                commandDictionary[""] = $"\"./{stream.HlsPlaylistFilename}.m3u8\"";
            }
            
            command.Append(commandDictionary.Aggregate("", (acc, pair) => $"{acc} {pair.Key} {pair.Value}"));
            
            stream.CreateFolder();
        }
        
        foreach (var stream in Container!.AudioStreams)
        {
            var commandDictionary = new Dictionary<string, dynamic>();
            var index = Container!.AudioStreams.IndexOf(stream);
            stream.AddToDictionary(commandDictionary, index);
            
            foreach (var parameter in Container._extraParameters ?? new Dictionary<string, dynamic>())
            {
                commandDictionary[parameter.Key] = parameter.Value;
            }
            
            // commandDictionary["-t"] = 300;
            if (Container.ContainerDto.Name == VideoContainers.Hls)
            {
                commandDictionary["-hls_segment_filename"] = $"\"./{stream.HlsPlaylistFilename}_%05d.ts\"";
                commandDictionary[""] = $"\"./{stream.HlsPlaylistFilename}.m3u8\"";
            }
            
            command.Append(commandDictionary.Aggregate("", (acc, pair) => $"{acc} {pair.Key} {pair.Value}"));
            
            stream.CreateFolder();
        }
        
        foreach (var stream in Container!.SubtitleStreams)
        {
            var commandDictionary = new Dictionary<string, dynamic>();
            var index = Container!.SubtitleStreams.IndexOf(stream);
            stream.AddToDictionary(commandDictionary, index);
            
            foreach (var parameter in Container._extraParameters ?? new Dictionary<string, dynamic>())
            {
                commandDictionary[parameter.Key] = parameter.Value;
            }
            
            // commandDictionary["-t"] = 300;
            if (Container.ContainerDto.Name == VideoContainers.Hls)
            {
                commandDictionary["-hls_segment_filename"] = $"\"./{stream.HlsPlaylistFilename}_%05d.vtt\"";
                commandDictionary[""] = $"\"./{stream.HlsPlaylistFilename}.m3u8\"";
            }
            
            command.Append(commandDictionary.Aggregate("", (acc, pair) => $"{acc} {pair.Key} {pair.Value}"));
            
            stream.CreateFolder();
        }
        
        foreach (var stream in Container.ImageStreams)
        {
            var commandDictionary = new Dictionary<string, dynamic>();
            
            var index = Container!.ImageStreams.IndexOf(stream);
            
            stream.AddToDictionary(commandDictionary, index);
            
            // commandDictionary["-t"] = 300;
            
            if (Container!.ContainerDto.Name == VideoContainers.Hls)
            {
                commandDictionary[""] = $"\"./{stream.Filename}/{stream.Filename}-%04d.jpg\"";
            }
            
            command.Append(commandDictionary.Aggregate("", (acc, pair) => $"{acc} {pair.Key} {pair.Value}"));
            
            stream.CreateFolder();
        }
        
        command.Append(" ");
        // command.Append(" 2>&1 ");
        return command.ToString();
    }

}