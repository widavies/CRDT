namespace CRDT; 

// 2-Phase set
// Elements can be added freely, but only removed once
// Downside: Have to store tombstones forever, can mitigate with tombstone pruning
// Downside 2: Can't re-add removed values
public class PSet<T> {
    private readonly GSet<T> _added = new();
    private readonly GSet<T> _removed = new();

    public ISet<T> Value {
        get {
            var working = _added.Values;
            working.RemoveWhere(x => _removed.Values.Contains(x));
            return working;
        }
    }

    public void Add(T value) {
        _added.Add(value);
    }

    public void Remove(T value) {
        _removed.Add(value);
    }

    public void MergeInPlace(PSet<T> other) {
        _added.MergeInPlace(other._added);
        _removed.MergeInPlace(other._removed);
    }
}