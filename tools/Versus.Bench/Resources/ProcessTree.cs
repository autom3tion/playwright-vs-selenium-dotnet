namespace Versus.Bench.Resources;

public static class ProcessTree
{
    /// <summary>The root and its descendants, each once. A pid already visited is skipped, so a parent cycle (stale parent ids) ends the walk.</summary>
    public static IReadOnlyList<ProcessSample> Of(IReadOnlyList<ProcessSample> all, int rootPid)
    {
        var children = all.ToLookup(p => p.ParentPid);
        var tree = new List<ProcessSample>();
        var visited = new HashSet<int>();
        var pending = new Queue<ProcessSample>(all.Where(p => p.Pid == rootPid));

        while (pending.TryDequeue(out var process))
        {
            if (!visited.Add(process.Pid))
            {
                continue;
            }

            tree.Add(process);
            foreach (var child in children[process.Pid])
            {
                pending.Enqueue(child);
            }
        }

        return tree;
    }
}
