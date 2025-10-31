using System.Text;
using VeritoolCollector.Models;

namespace VeritoolCollector.Writers;

internal static class CsvSnapshotWriter
{
    public static void Write(RunContext context, IReadOnlyDictionary<string, string> data, IReadOnlyCollection<CollectionOutcome> artifacts)
    {
        var snapshotPath = Path.Combine(context.DestinationRoot, "checklist_snapshot.csv");
        using var writer = new StreamWriter(snapshotPath, false, Encoding.UTF8);
        writer.WriteLine("Key,Value");

        foreach (var kvp in data.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase))
        {
            writer.WriteLine($"{Escape(kvp.Key)},{Escape(kvp.Value)}");
        }

        writer.WriteLine();
        writer.WriteLine("ArtifactKey,Source,Destination,Status,Message");
        foreach (var artifact in artifacts)
        {
            writer.WriteLine(string.Join(',',
                Escape(artifact.Key),
                Escape(artifact.Source),
                Escape(artifact.Destination),
                Escape(artifact.Status.ToString()),
                Escape(artifact.Message ?? string.Empty)));
        }

        context.Logger.Info($"CSV snapshot written to {snapshotPath}");
    }

    private static string Escape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return '"' + value.Replace("\"", "\"\"") + '"';
        }

        return value;
    }
}

