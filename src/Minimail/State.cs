using System.Collections.Concurrent;

namespace Minimail;

public static class State
{
    public static ConcurrentDictionary<string, object?> Whitelist { get; set; } = default!;
}