namespace VeritoolCollector;

internal sealed class CliOptions
{
    private CliOptions()
    {
    }

    public string? DestinationPath { get; private set; }
    public string? ConfigPath { get; private set; }
    public string? ChecklistPath { get; private set; }
    public Dictionary<string, string> MetadataOverrides { get; } = new(StringComparer.OrdinalIgnoreCase);

    public static CliOptions Parse(string[] args)
    {
        var options = new CliOptions();

        var i = 0;
        while (i < args.Length)
        {
            var arg = args[i];
            switch (arg)
            {
                case "--dest":
                case "--destination":
                    options.DestinationPath = RequireValue(args, ++i, arg);
                    break;
                case "--config":
                    options.ConfigPath = RequireValue(args, ++i, arg);
                    break;
                case "--checklist":
                    options.ChecklistPath = RequireValue(args, ++i, arg);
                    break;
                case "--meta":
                    var raw = RequireValue(args, ++i, arg);
                    AddMetadata(options, raw);
                    break;
                default:
                    if (arg.StartsWith("--meta:"))
                    {
                        var value = arg.Substring("--meta:".Length);
                        AddMetadata(options, value);
                    }
                    else if (arg is "-h" or "--help")
                    {
                        throw new ArgumentException("Help requested.");
                    }
                    else
                    {
                        throw new ArgumentException($"Unrecognized argument '{arg}'.");
                    }
                    break;
            }

            i++;
        }

        return options;
    }

    private static string RequireValue(string[] args, int index, string option)
    {
        if (index >= args.Length)
        {
            throw new ArgumentException($"Option {option} requires a value.");
        }

        return args[index];
    }

    private static void AddMetadata(CliOptions options, string raw)
    {
        var separator = raw.IndexOf('=');
        if (separator <= 0 || separator == raw.Length - 1)
        {
            throw new ArgumentException($"Metadata override must be in key=value format. Received '{raw}'.");
        }

        var key = raw[..separator].Trim();
        var value = raw[(separator + 1)..].Trim();
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Metadata key cannot be empty.");
        }

        options.MetadataOverrides[key] = value;
    }

    public static void PrintUsage()
    {
        Console.WriteLine("VeritoolCollector usage:");
        Console.WriteLine("  VeritoolCollector.exe [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --dest|--destination <path>   Destination folder for collected artifacts");
        Console.WriteLine("  --config <path>                Path to configuration JSON (defaults to veritool.config.json)");
        Console.WriteLine("  --checklist <path>             Path to checklist template to update");
        Console.WriteLine("  --meta key=value               Metadata override (repeat for multiple entries)");
    }
}

