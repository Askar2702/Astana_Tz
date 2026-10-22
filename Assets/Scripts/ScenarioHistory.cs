
using System.Collections.Generic;

public class ScenarioHistory
{
    private readonly List<ScenarioLogEntry> _entries = new();

    public IReadOnlyList<ScenarioLogEntry> Entries => _entries;

    public void Add(ScenarioLogEntry entry)
    {
        if (entry == null)
            return;

        _entries.Add(entry);
    }

    public void Clear()
    {
        _entries.Clear();
    }
}
