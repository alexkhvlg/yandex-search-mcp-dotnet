using System.Globalization;

namespace yandex_search_mcp_dotnet.Services;

public sealed class LogFileWriter
{
    private readonly string _logsDir;
    private readonly Lock _lock = new();

    public LogFileWriter()
    {
        _logsDir = Path.Combine(AppContext.BaseDirectory, "logs");
        Directory.CreateDirectory(_logsDir);
    }

    public void Write(string toolName, string context, string? result, string? error)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        var suffix = error is not null
            ? $"Error: {error}"
            : $"Result: {Truncate(result ?? "")}";

        var line = $"[{timestamp}] {toolName}: {context}. {suffix}{Environment.NewLine}";

        var fileName = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ".log";
        var filePath = Path.Combine(_logsDir, fileName);

        lock (_lock)
        {
            File.AppendAllText(filePath, line);
        }
    }

    private static string Truncate(string value)
    {
        if (value.Length <= 50)
        {
            return value;
        }

        return value[..50];
    }
}
