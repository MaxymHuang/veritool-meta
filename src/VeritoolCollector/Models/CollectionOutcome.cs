namespace VeritoolCollector.Models;

internal sealed class CollectionOutcome
{
    public CollectionOutcome(string key, string source, string destination, CollectionStatus status, string? message = null)
    {
        Key = key;
        Source = source;
        Destination = destination;
        Status = status;
        Message = message;
    }

    public string Key { get; }
    public string Source { get; }
    public string Destination { get; }
    public CollectionStatus Status { get; }
    public string? Message { get; }
}

internal enum CollectionStatus
{
    Success,
    Missing,
    Error
}

