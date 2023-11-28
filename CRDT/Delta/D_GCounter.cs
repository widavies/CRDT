namespace CRDT.Delta;

// A delta-state GCounter
// The concept here is that a GCounter can be split into its delta and the delta can be sent over instead
public class D_GCounter {
    public readonly Dictionary<int, int> Values = new();
    internal D_GCounter? _delta; // Delta.Delta is always null

    public int Value => Values.Sum(x => x.Value);

    public void Increment(int replicaId) {
        if (Values.TryGetValue(replicaId, out var value)) {
            Values[replicaId] = value + 1;
        } else {
            Values[replicaId] = 1;
        }

        if (_delta == null) {
            _delta = new D_GCounter();
        }

        _delta?.Increment(replicaId);
    }

    public void MergeInPlace(D_GCounter other) {
        // Pairwise maximize of each value
        foreach (var kv in other.Values) {
            if (Values.TryGetValue(kv.Key, out var value)) {
                Values[kv.Key] = Math.Max(kv.Value, value);
            } else {
                Values[kv.Key] = kv.Value;
            }
        }

        // Next, work out the delta
        if (_delta != null && other._delta != null) {
            _delta.MergeInPlace(other._delta);
        } else if (_delta == null && other._delta != null) {
            _delta = other._delta;
        }
    }

    // Split (full, delta)
    // This just makes it easy to send over the delta to merge with another guy
    public (D_GCounter, D_GCounter?) Split() {
        return (this, _delta);
    }
}