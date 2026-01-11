using NoMercy.Database.Models;
using NoMercy.EncoderV2.FFmpeg;
using NoMercy.EncoderV2.Hardware;
using NoMercy.EncoderV2.Specifications.HLS;
using NoMercy.EncoderV2.Streams;
using NoMercy.NmSystem.Capabilities;

namespace NoMercy.EncoderV2.Services;

/// <summary>
/// High-level encoding service that orchestrates the complete encoding process
/// This is the main entry point for encoding operations in EncoderV2
/// </summary>
public interface IEncodingService
{
    /// <summary>
    /// Encodes a media file using the specified profile
    /// Handles analysis, command building, execution, and playlist generation
    /// </summary>
    Task<EncodingResult> EncodeAsync(
        string inputFile,
        string outputFolder,
        EncoderProfile profile,
        Action<string>? progressCallback = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Encodes with V1-compatible separate stream output
    /// </summary>
    Task<EncodingResult> EncodeWithSeparateStreamsAsync(
        string inputFile,
        string outputFolder,
        string baseFilename,
        EncoderProfile profile,
        Action<string>? progressCallback = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Quick preview encoding with time limits
    /// </summary>
    Task<EncodingResult> EncodePreviewAsync(
        string inputFile,
        string outputFolder,
        EncoderProfile profile,
        TimeSpan seekStart,
        TimeSpan duration,
        Action<string>? progressCallback = null,
        CancellationToken cancellationToken = default);
}

public class EncodingService : IEncodingService
{
    private readonly IStreamAnalyzer _streamAnalyzer;
    private readonly IHardwareAccelerationService _hardwareService;
    private readonly ICodecSelector _codecSelector;
    private readonly IFFmpegService _ffmpegService;
    private readonly IHLSOutputOrchestrator _hlsOrchestrator;

    public EncodingService(
        IStreamAnalyzer streamAnalyzer,
        IHardwareAccelerationService hardwareService,
        ICodecSelector codecSelector,
        IFFmpegService ffmpegService,
        IHLSOutputOrchestrator hlsOrchestrator)
    {
        _streamAnalyzer = streamAnalyzer;
        _hardwareService = hardwareService;
        _codecSelector = codecSelector;
        _ffmpegService = ffmpegService;
        _hlsOrchestrator = hlsOrchestrator;
    }

    public async Task<EncodingResult> EncodeAsync(
        string inputFile,
        string outputFolder,
        EncoderProfile profile,
        Action<string>? progressCallback = null,
        CancellationToken cancellationToken = default)
    {
        // Validate inputs
        if (!File.Exists(inputFile))
        {
            return EncodingResult.Failure($"Input file not found: {inputFile}");
        }

        Directory.CreateDirectory(outputFolder);

        // Analyze media
        StreamAnalysis analysis = await _streamAnalyzer.AnalyzeAsync(inputFile, cancellationToken);

        // Get hardware accelerators
        List<GpuAccelerator> accelerators = _hardwareService.GetAvailableAccelerators();

        // Build FFmpeg command
        FFmpegCommandBuilder commandBuilder = new(
            analysis,
            profile,
            accelerators,
            inputFile,
            outputFolder,
            _codecSelector,
            HLSOutputMode.Combined);

        string command = commandBuilder.BuildCommand();

        // Execute encoding
        FFmpegExecutionResult executionResult = await _ffmpegService.ExecuteAsync(
            command,
            outputFolder,
            progressCallback,
            cancellationToken);

        return new EncodingResult
        {
            Success = executionResult.Success,
            OutputPath = outputFolder,
            Duration = executionResult.ExecutionTime,
            ErrorMessage = executionResult.ErrorMessage,
            ExitCode = executionResult.ExitCode
        };
    }

    public async Task<EncodingResult> EncodeWithSeparateStreamsAsync(
        string inputFile,
        string outputFolder,
        string baseFilename,
        EncoderProfile profile,
        Action<string>? progressCallback = null,
        CancellationToken cancellationToken = default)
    {
        // Validate inputs
        if (!File.Exists(inputFile))
        {
            return EncodingResult.Failure($"Input file not found: {inputFile}");
        }

        Directory.CreateDirectory(outputFolder);

        // Analyze media
        StreamAnalysis analysis = await _streamAnalyzer.AnalyzeAsync(inputFile, cancellationToken);

        // Get hardware accelerators
        List<GpuAccelerator> accelerators = _hardwareService.GetAvailableAccelerators();

        // Create HLS output structure
        HLSSpecification hlsSpec = new()
        {
            Version = 3,
            TargetDuration = 10,
            SegmentDuration = 6,
            PlaylistType = "VOD",
            IndependentSegments = true
        };

        HLSOutputStructure outputStructure = await _hlsOrchestrator.CreateOutputStructureFromStreamAnalysisAsync(
            outputFolder,
            baseFilename,
            analysis,
            hlsSpec);

        // Build FFmpeg command with separate streams mode
        FFmpegCommandBuilder commandBuilder = new(
            analysis,
            profile,
            accelerators,
            inputFile,
            outputFolder,
            _codecSelector,
            HLSOutputMode.SeparateStreams);

        commandBuilder.SetHLSOutputStructure(outputStructure);
        string command = commandBuilder.BuildCommand();

        // Execute encoding
        FFmpegExecutionResult executionResult = await _ffmpegService.ExecuteAsync(
            command,
            outputFolder,
            progressCallback,
            cancellationToken);

        // Generate playlists
        if (executionResult.Success || executionResult.ExitCode == 0)
        {
            await _hlsOrchestrator.GeneratePlaylistsAsync(outputStructure, analysis.Duration);
        }

        return new EncodingResult
        {
            Success = executionResult.Success,
            OutputPath = outputFolder,
            Duration = executionResult.ExecutionTime,
            ErrorMessage = executionResult.ErrorMessage,
            ExitCode = executionResult.ExitCode,
            HLSOutputStructure = outputStructure
        };
    }

    public async Task<EncodingResult> EncodePreviewAsync(
        string inputFile,
        string outputFolder,
        EncoderProfile profile,
        TimeSpan seekStart,
        TimeSpan duration,
        Action<string>? progressCallback = null,
        CancellationToken cancellationToken = default)
    {
        // Validate inputs
        if (!File.Exists(inputFile))
        {
            return EncodingResult.Failure($"Input file not found: {inputFile}");
        }

        Directory.CreateDirectory(outputFolder);

        // Analyze media
        StreamAnalysis analysis = await _streamAnalyzer.AnalyzeAsync(inputFile, cancellationToken);

        // Get hardware accelerators
        List<GpuAccelerator> accelerators = _hardwareService.GetAvailableAccelerators();

        // Build FFmpeg command with time range
        FFmpegCommandBuilder commandBuilder = new(
            analysis,
            profile,
            accelerators,
            inputFile,
            outputFolder,
            _codecSelector,
            HLSOutputMode.Combined);

        commandBuilder.SetInputTimeRange(seekStart, duration);
        string command = commandBuilder.BuildCommand();

        // Execute encoding
        FFmpegExecutionResult executionResult = await _ffmpegService.ExecuteAsync(
            command,
            outputFolder,
            progressCallback,
            cancellationToken);

        return new EncodingResult
        {
            Success = executionResult.Success,
            OutputPath = outputFolder,
            Duration = executionResult.ExecutionTime,
            ErrorMessage = executionResult.ErrorMessage,
            ExitCode = executionResult.ExitCode
        };
    }
}

/// <summary>
/// Result of an encoding operation
/// </summary>
public class EncodingResult
{
    public bool Success { get; set; }
    public string OutputPath { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public string? ErrorMessage { get; set; }
    public int ExitCode { get; set; }
    public HLSOutputStructure? HLSOutputStructure { get; set; }

    public static EncodingResult Failure(string errorMessage)
    {
        return new EncodingResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            ExitCode = -1
        };
    }
}
