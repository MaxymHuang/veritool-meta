using System.Management;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using VeritoolCollector.Models;

namespace VeritoolCollector.Collectors;

internal static class SystemInfoCollector
{
    public static SystemInfo Collect(Logger logger)
    {
        if (!OperatingSystem.IsWindows())
        {
            var description = RuntimeInformation.OSDescription;
            logger.Warn($"System information collection requires Windows APIs. Current platform: {description}. Skipping detailed WMI query.");
            return new SystemInfo(description, string.Empty, null, Array.Empty<GraphicsAdapterInfo>());
        }

        return CollectWindows(logger);
    }

    [SupportedOSPlatform("windows")]
    private static SystemInfo CollectWindows(Logger logger)
    {
        logger.Info("Gathering system information...");

        var osName = string.Empty;
        var osVersion = string.Empty;
        using (var searcher = new ManagementObjectSearcher("SELECT Caption, Version FROM Win32_OperatingSystem"))
        {
            foreach (var obj in searcher.Get())
            {
                osName = obj["Caption"]?.ToString() ?? string.Empty;
                osVersion = obj["Version"]?.ToString() ?? string.Empty;
            }
        }

        double? totalRamGb = null;
        using (var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem"))
        {
            foreach (var obj in searcher.Get())
            {
                if (obj["TotalPhysicalMemory"] is ulong memoryBytes)
                {
                    totalRamGb = Math.Round(memoryBytes / 1024d / 1024d / 1024d, 2);
                }
            }
        }

        var gpus = new List<GraphicsAdapterInfo>();
        using (var searcher = new ManagementObjectSearcher("SELECT Name, AdapterRAM, DriverVersion FROM Win32_VideoController"))
        {
            foreach (var obj in searcher.Get())
            {
                var name = obj["Name"]?.ToString() ?? "Unknown";
                double? ramGb = null;
                if (obj["AdapterRAM"] is uint adapterRam)
                {
                    ramGb = Math.Round(adapterRam / 1024d / 1024d / 1024d, 2);
                }
                else if (obj["AdapterRAM"] is ulong adapterRam64)
                {
                    ramGb = Math.Round(adapterRam64 / 1024d / 1024d / 1024d, 2);
                }

                var driver = obj["DriverVersion"]?.ToString() ?? string.Empty;
                gpus.Add(new GraphicsAdapterInfo(name, ramGb, driver));
            }
        }

        return new SystemInfo(osName, osVersion, totalRamGb, gpus);
    }
}

