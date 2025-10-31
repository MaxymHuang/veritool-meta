using VeritoolCollector.Configuration;

namespace VeritoolCollector.Models;

internal sealed class RunContext
{
    public RunContext(string destinationRoot, CliOptions options, AppConfig config, Logger logger)
    {
        DestinationRoot = destinationRoot;
        Options = options;
        Config = config;
        Logger = logger;

        LogFilePath = Path.Combine(DestinationRoot, "logs", "collector.log");
        ArtifactsDirectory = Path.Combine(DestinationRoot, "artifacts");
        Directory.CreateDirectory(ArtifactsDirectory);

        if (!string.IsNullOrWhiteSpace(options.ChecklistPath))
        {
            ChecklistTemplatePath = Path.GetFullPath(options.ChecklistPath!);
        }
    }

    public string DestinationRoot { get; }
    public CliOptions Options { get; }
    public AppConfig Config { get; }
    public Logger Logger { get; }
    public string LogFilePath { get; }
    public string ArtifactsDirectory { get; }
    public string? ChecklistTemplatePath { get; }
    public string? ChecklistOutputPath { get; set; }
    public string SystemInfoPath => Path.Combine(DestinationRoot, "system_info.txt");
}

