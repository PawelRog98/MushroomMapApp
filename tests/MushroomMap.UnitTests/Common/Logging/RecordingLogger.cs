using Microsoft.Extensions.Logging;

namespace MushroomMap.UnitTests.Common.Logging;

public sealed class LogEntry
{
    public LogLevel Level { get; init; }
    public string Message { get; init; } = string.Empty;
    public Exception? Exception { get; init; }
}

public sealed class RecordingLogger<T> : ILogger<T>
{
    public List<LogEntry> Entries { get; } = [];

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        Entries.Add(new LogEntry
        {
            Level = logLevel,
            Message = formatter(state, exception),
            Exception = exception
        });
    }
}
