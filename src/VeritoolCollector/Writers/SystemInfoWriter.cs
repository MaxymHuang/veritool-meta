using System.Text;
using VeritoolCollector.Models;

namespace VeritoolCollector.Writers;

internal static class SystemInfoWriter
{
    public static void Write(SystemInfo systemInfo, RunContext context)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Windows Device Information");
        sb.AppendLine("=========================");
        sb.AppendLine();
        sb.AppendLine($"OS Name: {systemInfo.OsName}");
        sb.AppendLine($"OS Version: {systemInfo.OsVersion}");
        if (systemInfo.TotalRamGb.HasValue)
        {
            sb.AppendLine($"Total RAM: {systemInfo.TotalRamGb:F2} GB");
        }

        sb.AppendLine();
        sb.AppendLine("Graphics Adapters:");
        foreach (var adapter in systemInfo.GraphicsAdapters)
        {
            var memory = adapter.MemoryGb.HasValue ? $"{adapter.MemoryGb:F2} GB" : "Unknown";
            sb.AppendLine($"  - {adapter.Name} (Memory: {memory}, Driver: {adapter.DriverVersion})");
        }

        File.WriteAllText(context.SystemInfoPath, sb.ToString());
        context.Logger.Info($"System information saved to {context.SystemInfoPath}");
    }
}

