using VeritoolCollector.Configuration;
using VeritoolCollector.Collectors;
using VeritoolCollector.Models;
using VeritoolCollector.Writers;

namespace VeritoolCollector;

internal static class Program
{
    private static int Main(string[] args)
    {
        var logger = new Logger();

        CliOptions options;
        try
        {
            options = CliOptions.Parse(args);
        }
        catch (ArgumentException ex)
        {
            logger.Error(ex.Message);
            CliOptions.PrintUsage();
            return 1;
        }

        try
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var configPath = options.ConfigPath ?? Path.Combine(baseDirectory, "veritool.config.json");
            var config = AppConfigLoader.Load(configPath, logger);

            var destinationRoot = DetermineDestination(options, logger);
            Directory.CreateDirectory(destinationRoot);

            logger.Info($"Destination folder: {destinationRoot}");

            var runContext = new RunContext(destinationRoot, options, config, logger);

            var systemInfo = SystemInfoCollector.Collect(logger);
            SystemInfoWriter.Write(systemInfo, runContext);

            var artifactResults = ArtifactCollector.Execute(runContext);

            var dataSnapshot = MetadataBuilder.Build(runContext, systemInfo, artifactResults);

            if (options.ChecklistPath is not null)
            {
                ChecklistUpdater.TryUpdate(runContext, dataSnapshot, logger);
                if (!string.IsNullOrWhiteSpace(runContext.ChecklistOutputPath))
                {
                    dataSnapshot["ChecklistPath"] = runContext.ChecklistOutputPath!;
                }
            }
            CsvSnapshotWriter.Write(runContext, dataSnapshot, artifactResults);

            logger.Info("Collection completed successfully.");
            logger.FlushTo(runContext.LogFilePath);
            return 0;
        }
        catch (Exception ex)
        {
            logger.Error($"Unexpected error: {ex.Message}");
            logger.Debug(ex.ToString());
            logger.FlushTo(Path.Combine(Environment.CurrentDirectory, "collector-error.log"));
            return 2;
        }
    }

    private static string DetermineDestination(CliOptions options, Logger logger)
    {
        if (!string.IsNullOrWhiteSpace(options.DestinationPath))
        {
            return Path.GetFullPath(options.DestinationPath!);
        }

        var defaultRoot = Path.Combine(Environment.CurrentDirectory, "collector-output");
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var destination = Path.Combine(defaultRoot, timestamp);
        logger.Debug($"Destination not provided. Using default path: {destination}");
        return destination;
    }
}

