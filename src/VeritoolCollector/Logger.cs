using System.Text;

namespace VeritoolCollector;

internal sealed class Logger
{
    private readonly StringBuilder _buffer = new();

    public void Info(string message) => Write("INFO", message);
    public void Warn(string message) => Write("WARN", message);
    public void Error(string message) => Write("ERROR", message);
    public void Debug(string message) => Write("DEBUG", message);

    private void Write(string level, string message)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var formatted = $"[{timestamp}] {level,-5} {message}";
        Console.WriteLine(formatted);
        _buffer.AppendLine(formatted);
    }

    public void FlushTo(string path)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, _buffer.ToString());
    }
}

