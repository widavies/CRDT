namespace CRDT;

// A grow only counter
public class GCounter {
    public readonly Dictionary<int, int> Values = new();

    // Retrieve the value for this counter
    public int Value => Values.Sum(x => x.Value);

    // Merge a counter into this one
    public void MergeInPlace(GCounter other) {
        // Pairwise maximize of each value
        foreach (var kv in other.Values) {
            if (Values.TryGetValue(kv.Key, out var value)) {
                Values[kv.Key] = Math.Max(kv.Value, value);
            } else {
                Values[kv.Key] = kv.Value;
            }
        }
    }

    public GCounter Merge(GCounter other) {
        GCounter c1 = new();
        c1.MergeInPlace(this);
        c1.MergeInPlace(other);
        return c1;
    }

    // Note - think about scoping the entire CRDT library to a replicaId so it doesn't need
    // to get passed to every function
    public void Increment(int replicaId) {
        if (Values.TryGetValue(replicaId, out var value)) {
            Values[replicaId] = value + 1;
        } else {
            Values[replicaId] = 1;
        }
    }
}