using System.Globalization;
using System.Text.RegularExpressions;

namespace NoMercy.MediaSources.OpticalMedia.Dto;

public partial class BluRayPlaylist
{
    public string CompleteName { get; set; } = string.Empty;
    public string playlistId { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public TimeSpan Duration { get; set; }
    public string OverallBitRate { get; set; } = string.Empty;
    public List<VideoTrack> VideoTracks { get; set; } = [];
    public List<AudioTrack> AudioTracks { get; set; } = [];
    public List<SubtitleTrack> SubtitleTracks { get; set; } = [];
    public List<Chapter> Chapters { get; set; } = [];

    public static BluRayPlaylist Parse(string input)
    {
        BluRayPlaylist playlist = new();
        string[] lines = input.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);

        VideoTrack? currentVideo = null;
        AudioTrack? currentAudio = null;
        SubtitleTrack? currentSubtitle = null;
        Chapter? currentChapter = null;
        int index = 0;

        foreach (string line in lines)
        {
            string value = line.Split(':', 2).LastOrDefault()?.Trim() ?? string.Empty;

            if (line.StartsWith("Complete name"))
            {
                playlist.CompleteName = value;
                playlist.playlistId = value.Split('\\').LastOrDefault()
                    ?.Split('.').FirstOrDefault() ?? string.Empty;
            }
            else if (line.StartsWith("Format "))
                playlist.Format = value;
            else if (line.StartsWith("File size"))
                playlist.FileSize = ParseInt(value);
            else if (line.StartsWith("Duration"))
                playlist.Duration = ParseDuration(value);
            else if (line.StartsWith("Overall bit rate"))
                playlist.OverallBitRate = value;
            else if (line.StartsWith("Video"))
            {
                currentVideo = new()
                {
                    StreamIndex = index++
                };
            }
            else if (line.StartsWith("Audio"))
            {
                currentAudio = new()
                {
                    StreamIndex = index++
                };
            }
            else if (line.StartsWith("Text"))
            {
                currentSubtitle = new()
                {
                    StreamIndex = index++
                };
            }
            else if (line.StartsWith("Menu"))
                currentChapter = new();

            if (currentVideo != null)
            {
                if (line.StartsWith("ID"))
                    currentVideo.Id = ParseInt(value);
                else if (line.StartsWith("Format") && currentVideo.Format == null)
                    currentVideo.Format = value;
                else if (line.StartsWith("Format/Info"))
                    currentVideo.FormatInfo = value;
                else if (line.StartsWith("Width"))
                    currentVideo.Width = ParseInt(value);
                else if (line.StartsWith("Height"))
                    currentVideo.Height = ParseInt(value);
                else if (line.StartsWith("Display aspect ratio"))
                    currentVideo.DisplayAspectRatio = value;
                else if (line.StartsWith("Frame rate"))
                {
                    currentVideo.FrameRate =
                        double.Parse(Regex.Match(line, "[0-9.]+").Value, CultureInfo.InvariantCulture);
                    playlist.VideoTracks.Add(currentVideo);
                    currentVideo = null;
                }
            }

            if (currentAudio != null)
            {
                if (line.StartsWith("ID"))
                    currentAudio.Id = ParseInt(value);
                else if (line.StartsWith("Format") && currentAudio.Format == null)
                    currentAudio.Format = value;
                else if (line.StartsWith("Format/Info"))
                    currentAudio.FormatInfo = value;
                else if (line.StartsWith("Channel(s)"))
                    currentAudio.Channels = ParseInt(value);
                else if (line.StartsWith("Sampling rate"))
                    currentAudio.SamplingRate = ParseInt(value);
                else if (line.StartsWith("Compression mode"))
                    currentAudio.CompressionMode = value;
                else if (line.StartsWith("Duration"))
                    currentAudio.Duration = ParseDuration(value);
                else if (line.StartsWith("Language"))
                {
                    currentAudio.Language = value;
                    playlist.AudioTracks.Add(currentAudio);
                    currentAudio = null;
                }
            }

            if (currentSubtitle != null)
            {
                if (line.StartsWith("ID"))
                    currentSubtitle.Id = ParseInt(value);
                else if (line.StartsWith("Format"))
                    currentSubtitle.Format = value;
                else if (line.StartsWith("Duration"))
                    currentSubtitle.Duration = ParseDuration(value);
                else if (line.StartsWith("Language"))
                {
                    currentSubtitle.Language = value;
                    playlist.SubtitleTracks.Add(currentSubtitle);
                    currentSubtitle = null;
                }
            }

            if (currentChapter == null) continue;
            
            Chapter? chapter = ParseChapter(line);
            if (chapter != null)
            {
                playlist.Chapters.Add(chapter);
            }
        }

        return playlist;
    }

    private static TimeSpan ParseDuration(string input)
    {
        Match match = Regex.Match(input, @"(?:(\d+)\s*hour[s]?)?\s*(?:(\d+)\s*min[s]?)?\s*(?:(\d+)\s*s)");
        int hours = match.Groups[1].Success ? int.Parse(match.Groups[1].Value) : 0;
        int minutes = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 0;
        int seconds = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 0;
        return new(hours, minutes, seconds);
    }

    private static int ParseInt(string input)
    {
        return int.Parse(ValueRegex().Match(input).Value.Replace(" ", ""));
    }

    private static Chapter? ParseChapter(string line)
    {
        Match match = Regex.Match(line, @"(\d+:\d+:\d+.\d+)\s*:\s(.*)");
        if (match.Success)
        {
            return new()
            {
                Timestamp = TimeSpan.Parse(match.Groups[1].Value),
                Title = match.Groups[2].Value
            };
        }

        return null;
    }

    [GeneratedRegex(@"[\d\s]+")]
    private static partial Regex ValueRegex();
}

public class VideoTrack
{
    public int Id { get; set; }
    public int StreamIndex { get; set; }
    public string? Format { get; set; }
    public string FormatInfo { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public string DisplayAspectRatio { get; set; } = string.Empty;
    public double FrameRate { get; set; }
}

public class AudioTrack
{
    public int Id { get; set; }
    public int StreamIndex { get; set; }
    public string? Format { get; set; }
    public string FormatInfo { get; set; } = string.Empty;
    public string? CommercialName { get; set; }
    public TimeSpan Duration { get; set; }
    public int? Channels { get; set; }
    public int SamplingRate { get; set; } // Hz
    public string? CompressionMode { get; set; }
    public string Language { get; set; } = string.Empty;
}

public class SubtitleTrack
{
    public int Id { get; set; }
    public int StreamIndex { get; set; }
    public string Format { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public string Language { get; set; } = string.Empty;
}

public class Chapter
{
    public TimeSpan Timestamp { get; set; }
    public string Title { get; set; } = string.Empty;
}