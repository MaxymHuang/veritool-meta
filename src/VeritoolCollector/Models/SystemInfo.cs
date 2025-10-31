namespace VeritoolCollector.Models;

internal sealed record SystemInfo(
    string OsName,
    string OsVersion,
    double? TotalRamGb,
    IReadOnlyCollection<GraphicsAdapterInfo> GraphicsAdapters
);

internal sealed record GraphicsAdapterInfo(
    string Name,
    double? MemoryGb,
    string DriverVersion
);

