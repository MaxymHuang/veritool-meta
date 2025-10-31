using System.Text.Json;
using System.Text.Json.Serialization;

namespace VeritoolCollector.Configuration;

internal sealed class AppConfig
{
    public List<FileCopyTask> FileCopies { get; init; } = new();
    public List<DirectoryCopyTask> DirectoryCopies { get; init; } = new();
    public List<RegistryExportTask> RegistryExports { get; init; } = new();
    public Dictionary<string, Dictionary<string, string>> ChecklistBindings { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string> DefaultMetadata { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}

internal sealed class FileCopyTask : CopyTaskBase
{
}

internal sealed class DirectoryCopyTask : CopyTaskBase
{
}

internal abstract class CopyTaskBase
{
    public string Key { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
    public string? DestinationName { get; init; }
    public bool Required { get; init; }
    public string? ChecklistPathKey { get; init; }
    public string? ChecklistStatusKey { get; init; }
    public string? ChecklistNotesKey { get; init; }
    public string? Notes { get; init; }
}

internal sealed class RegistryExportTask
{
    public string Key { get; init; } = string.Empty;
    public string RegistryPath { get; init; } = string.Empty;
    public string ExportFileName { get; init; } = string.Empty;
    public string? ChecklistStatusKey { get; init; }
    public string? ChecklistPathKey { get; init; }
    public string? ChecklistNotesKey { get; init; }
    public string? Notes { get; init; }
}

internal static class AppConfigLoader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        PropertyNameCaseInsensitive = true
    };

    public static AppConfig Load(string path, VeritoolCollector.Logger logger)
    {
        if (!File.Exists(path))
        {
            logger.Warn($"Configuration file '{path}' not found. Using built-in defaults.");
            return CreateDefault();
        }

        try
        {
            var json = File.ReadAllText(path);
            var config = JsonSerializer.Deserialize<AppConfig>(json, Options);
            if (config is null)
            {
                logger.Warn("Configuration file was empty. Using defaults.");
                return CreateDefault();
            }

            return config;
        }
        catch (Exception ex)
        {
            logger.Warn($"Failed to parse configuration file: {ex.Message}. Using defaults.");
            return CreateDefault();
        }
    }

    private static AppConfig CreateDefault()
    {
        return new AppConfig
        {
            FileCopies = new List<FileCopyTask>
            {
                new()
                {
                    Key = "SoftwareLimit",
                    Source = @"C:\\TR7600\\TriMotion.cfg",
                    DestinationName = "TriMotion.cfg",
                    ChecklistPathKey = "SoftwareLimitPath",
                    ChecklistStatusKey = "SoftwareLimitStatus",
                    ChecklistNotesKey = "SoftwareLimitNotes",
                    Notes = "備份後連著Check List一起打包寄出"
                }
            },
            DirectoryCopies = new List<DirectoryCopyTask>
            {
                new()
                {
                    Key = "Library",
                    Source = @"C:\\TRI_AXI",
                    DestinationName = "TRI_AXI",
                    ChecklistPathKey = "LibraryPath",
                    ChecklistStatusKey = "LibraryStatus",
                    ChecklistNotesKey = "LibraryNotes",
                    Notes = "備份後連著Check List一起打包寄出"
                },
                new()
                {
                    Key = "Calibration",
                    Source = @"C:\\TR7600\\Log\\Calibration",
                    DestinationName = "Calibration",
                    ChecklistPathKey = "CalibrationPath",
                    ChecklistStatusKey = "CalibrationStatus",
                    ChecklistNotesKey = "CalibrationNotes",
                    Notes = "備份後連著Check List一起打包寄出"
                },
                new()
                {
                    Key = "GrayLevel",
                    Source = @"C:\\TR7600\\Log\\GrayLevel",
                    DestinationName = "GrayLevel",
                    ChecklistPathKey = "GrayLevelPath",
                    ChecklistStatusKey = "GrayLevelStatus",
                    ChecklistNotesKey = "GrayLevelNotes",
                    Notes = "備份後連著Check List一起打包寄出"
                }
            },
            RegistryExports = new List<RegistryExportTask>
            {
                new()
                {
                    Key = "TriRegistry",
                    RegistryPath = @"HKEY_LOCAL_MACHINE\\SOFTWARE\\TRI\\TR7600",
                    ExportFileName = "TR7600.reg",
                    ChecklistPathKey = "RegistryPath",
                    ChecklistStatusKey = "RegistryStatus",
                    ChecklistNotesKey = "RegistryNotes",
                    Notes = "備份後連著Check List一起打包寄出"
                }
            },
            ChecklistBindings = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["FAE Check List"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["Date"] = "C3",
                    ["FAE"] = "C4",
                    ["Customer"] = "C5",
                    ["MachineModel"] = "C6",
                    ["SerialNumber"] = "C7",
                    ["TubeSerial"] = "C8",
                    ["CameraSerial"] = "C9",
                    ["ResolutionSpec"] = "C10",
                    ["Height"] = "C11",
                    ["WindowsOS"] = "C13",
                    ["RAM"] = "C14",
                    ["GraphicsCard"] = "C15",
                    ["AxiVersion"] = "C16",
                    ["PlcVersion"] = "C17",
                    ["SoftwareLimitPath"] = "C19",
                    ["SoftwareLimitStatus"] = "D19",
                    ["SoftwareLimitNotes"] = "F19",
                    ["LibraryPath"] = "C20",
                    ["LibraryStatus"] = "D20",
                    ["LibraryNotes"] = "F20",
                    ["RegistryPath"] = "C21",
                    ["RegistryStatus"] = "D21",
                    ["RegistryNotes"] = "F21",
                    ["CalibrationPath"] = "C22",
                    ["CalibrationStatus"] = "D22",
                    ["CalibrationNotes"] = "F22",
                    ["GrayLevelPath"] = "C23",
                    ["GrayLevelStatus"] = "D23",
                    ["GrayLevelNotes"] = "F23",
                }
            },
            DefaultMetadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["SoftwareLimitNotes"] = "備份後連著Check List一起打包寄出",
                ["LibraryNotes"] = "備份後連著Check List一起打包寄出",
                ["RegistryNotes"] = "備份後連著Check List一起打包寄出",
                ["CalibrationNotes"] = "備份後連著Check List一起打包寄出",
                ["GrayLevelNotes"] = "備份後連著Check List一起打包寄出"
            }
        };
    }
}

