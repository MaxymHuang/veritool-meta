using VeritoolCollector.Configuration;
using VeritoolCollector.Models;

namespace VeritoolCollector.Writers;

internal static class MetadataBuilder
{
    public static Dictionary<string, string> Build(RunContext context, SystemInfo systemInfo, IReadOnlyCollection<CollectionOutcome> artifactResults)
    {
        var data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var kvp in context.Config.DefaultMetadata)
        {
            data[kvp.Key] = kvp.Value;
        }

        foreach (var kvp in context.Options.MetadataOverrides)
        {
            data[kvp.Key] = kvp.Value;
        }

        if (!data.ContainsKey("Date"))
        {
            data["Date"] = DateTime.Now.ToString("yyyy/MM/dd");
        }

        data["WindowsOS"] = string.Join(" ", new[] { systemInfo.OsName, systemInfo.OsVersion }.Where(s => !string.IsNullOrWhiteSpace(s)));
        data["RAM"] = systemInfo.TotalRamGb.HasValue ? $"{systemInfo.TotalRamGb:F2} GB" : "";
        if (systemInfo.GraphicsAdapters.Count > 0)
        {
            data["GraphicsCard"] = string.Join("; ", systemInfo.GraphicsAdapters.Select(g =>
            {
                var memory = g.MemoryGb.HasValue ? $" ({g.MemoryGb:F2} GB)" : string.Empty;
                var driver = string.IsNullOrWhiteSpace(g.DriverVersion) ? string.Empty : $" - Driver {g.DriverVersion}";
                return g.Name + memory + driver;
            }));
        }

        data["SystemInfoPath"] = context.SystemInfoPath;

        foreach (var outcome in artifactResults)
        {
            var statusValue = outcome.Status switch
            {
                CollectionStatus.Success => "Ok",
                CollectionStatus.Missing => "Not Found",
                CollectionStatus.Error => "Error",
                _ => outcome.Status.ToString()
            };

            var baseKey = outcome.Key;
            if (!string.IsNullOrWhiteSpace(outcome.Destination))
            {
                data[$"{baseKey}Path"] = outcome.Destination;
            }

            data[$"{baseKey}Status"] = statusValue;

            if (!string.IsNullOrWhiteSpace(outcome.Message))
            {
                data[$"{baseKey}Notes"] = outcome.Message!;
            }
            else if (!data.ContainsKey($"{baseKey}Notes"))
            {
                var defaultNotes = context.Config.DefaultMetadata.GetValueOrDefault($"{baseKey}Notes");
                if (!string.IsNullOrWhiteSpace(defaultNotes))
                {
                    data[$"{baseKey}Notes"] = defaultNotes;
                }
            }
        }

        return data;
    }
}

