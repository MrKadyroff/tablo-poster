using System.Collections.Concurrent;

namespace LedImageUpdaterService.Services;

/// <summary>
/// Thread-safe in-memory circular log buffer used by the /api/led/logs endpoint.
/// Stores the last <see cref="MaxEntries"/> log records produced by LED services.
/// </summary>
public sealed class InMemoryLogStore
{
    private const int MaxEntries = 500;

    private readonly ConcurrentQueue<LogEntry> _entries = new();

    /// <summary>Adds a log record to the store. Oldest records are evicted when the buffer is full.</summary>
    public void Add(LogLevel level, string category, string message)
    {
        _entries.Enqueue(new LogEntry(DateTimeOffset.UtcNow, level, category, message));

        // Trim excess entries (allow small overshoot for performance)
        while (_entries.Count > MaxEntries)
            _entries.TryDequeue(out _);
    }

    /// <summary>Returns up to <paramref name="count"/> most recent entries.</summary>
    public IReadOnlyList<LogEntry> GetRecent(int count = 100)
    {
        var clamped = Math.Clamp(count, 1, MaxEntries);
        return _entries.TakeLast(clamped).ToList();
    }

    /// <summary>Clears all stored entries.</summary>
    public void Clear() => _entries.Clear();

    public record LogEntry(
        DateTimeOffset Timestamp,
        LogLevel Level,
        string Category,
        string Message);
}
