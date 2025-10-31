using System.Diagnostics;
using VeritoolCollector.Configuration;
using VeritoolCollector.Models;

namespace VeritoolCollector.Collectors;

internal static class ArtifactCollector
{
    public static IReadOnlyCollection<CollectionOutcome> Execute(RunContext context)
    {
        var results = new List<CollectionOutcome>();

        foreach (var file in context.Config.FileCopies)
        {
            results.Add(CopyFile(file, context));
        }

        foreach (var directory in context.Config.DirectoryCopies)
        {
            results.Add(CopyDirectory(directory, context));
        }

        foreach (var export in context.Config.RegistryExports)
        {
            results.Add(ExportRegistry(export, context));
        }

        return results;
    }

    private static CollectionOutcome CopyFile(FileCopyTask task, RunContext context)
    {
        return CopyWithHandler(task.Key, task.Source, task.DestinationName, context, File.Exists, (s, d) => File.Copy(s, d, overwrite: true), task.Notes);
    }

    private static CollectionOutcome CopyDirectory(DirectoryCopyTask task, RunContext context)
    {
        return CopyWithHandler(task.Key, task.Source, task.DestinationName, context, Directory.Exists, CopyDirectoryRecursive, task.Notes);
    }

    private static CollectionOutcome CopyWithHandler(string key, string source, string? destinationName, RunContext context, Func<string, bool> exists, Action<string, string> copyAction, string? notes)
    {
        try
        {
            if (!exists(source))
            {
                var message = $"Source not found: {source}";
                context.Logger.Warn(message);
                return new CollectionOutcome(key, source, string.Empty, CollectionStatus.Missing, message);
            }

            var destination = Path.Combine(context.ArtifactsDirectory, destinationName ?? Path.GetFileName(source));
            if (File.Exists(destination) || Directory.Exists(destination))
            {
                context.Logger.Debug($"Destination already exists. Overwriting: {destination}");
            }

            copyAction(source, destination);
            context.Logger.Info($"Copied {source} -> {destination}");
            return new CollectionOutcome(key, source, destination, CollectionStatus.Success, notes);
        }
        catch (Exception ex)
        {
            var message = $"Failed to copy {source}: {ex.Message}";
            context.Logger.Error(message);
            return new CollectionOutcome(key, source, string.Empty, CollectionStatus.Error, message);
        }
    }

    private static void CopyDirectoryRecursive(string sourceDir, string destinationDir)
    {
        if (!Directory.Exists(destinationDir))
        {
            Directory.CreateDirectory(destinationDir);
        }

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var dest = Path.Combine(destinationDir, Path.GetFileName(file));
            File.Copy(file, dest, overwrite: true);
        }

        foreach (var directory in Directory.GetDirectories(sourceDir))
        {
            var dest = Path.Combine(destinationDir, Path.GetFileName(directory));
            CopyDirectoryRecursive(directory, dest);
        }
    }

    private static CollectionOutcome ExportRegistry(RegistryExportTask task, RunContext context)
    {
        var destination = Path.Combine(context.ArtifactsDirectory, task.ExportFileName);
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "reg.exe",
                Arguments = $"export \"{task.RegistryPath}\" \"{destination}\" /y",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process is null)
            {
                const string message = "Unable to launch reg.exe";
                context.Logger.Error(message);
                return new CollectionOutcome(task.Key, task.RegistryPath, string.Empty, CollectionStatus.Error, message);
            }

            var stdout = process.StandardOutput.ReadToEnd();
            var stderr = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                var message = $"reg.exe exited with code {process.ExitCode}. {stderr}";
                context.Logger.Warn(message);
                return new CollectionOutcome(task.Key, task.RegistryPath, string.Empty, CollectionStatus.Missing, message);
            }

            context.Logger.Info($"Exported registry {task.RegistryPath} -> {destination}");
            return new CollectionOutcome(task.Key, task.RegistryPath, destination, CollectionStatus.Success, task.Notes);
        }
        catch (Exception ex)
        {
            var message = $"Failed to export registry {task.RegistryPath}: {ex.Message}";
            context.Logger.Error(message);
            return new CollectionOutcome(task.Key, task.RegistryPath, string.Empty, CollectionStatus.Error, message);
        }
    }
}

