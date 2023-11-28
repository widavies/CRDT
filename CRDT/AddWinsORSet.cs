namespace CRDT;

// This is called a "Add wins observed removed set" because:
// - When there are concurrent "add" and "remove" updates, the "add" wins
//        - We have to use a vector clock for this. If we didn't, then we can't really control whether an add / remove happens
//        - If we don't care about add wins, we could just use LWW registers for everything
// - "observed remove", which means removes are "observed" at a certain logical time, and can be canceled by a future addition
// can tombstones be eliminated using the one trick?
//          - yes, the added/remove set always keeps an up to date timestamp, so we only need to keep the "tombstone" as long as it's not re-added
// DOWNSIDE:
// - version vectors start to make up a lot of metadata
//      - either compress
//      - or, use dotted version vectors
//          - 
// NEXT:
// - Build a LWWMap!
//      - Use a ORSET<(k, LWWREG(V))) (comparison is only the key component)
public class AddWinsORSet<T> where T : notnull {
    private readonly Dictionary<T, VersionVector> _added = new();
    private readonly Dictionary<T, VersionVector> _removed = new();

    public ISet<T> Value {
        get {
            var dict = _added.Where(kv =>
                // Remove all values, where add happened before remove
                _removed.TryGetValue(kv.Key, out var other) && kv.Value.Compare(other) == VersionVectorComparisonResult.LessThan);

            // User only cares about the keys
            return dict.Select(x => x.Key).ToHashSet();
        }
    }

    public void Add(int replicaId, T value) {
        // In added set
        if (_added.TryGetValue(value, out var version1)) {
            version1.Increment(replicaId);
            _removed.Remove(value); // todo curious
        }
        // In removed set only
        else if (_removed.TryGetValue(value, out var version2)) {
            version2.Increment(replicaId);
            _added[value] = version2;
            _removed.Remove(value);
        }
        // In neither
        else {
            var version3 = new VersionVector();
            version3.Increment(replicaId);
            _added[value] = version3;
        }
    }

    public void Remove(int replicaId, T value) {
        // In added set
        if (_added.TryGetValue(value, out var version1)) {
            version1.Increment(replicaId);
            _removed[value] = version1;
            _added.Remove(value);
        }
        // In removed set only, keep it there, but make sure it has an up to date version id
        // because added could be more recent
        else if (_removed.TryGetValue(value, out var version2)) {
            _added.Remove(value);
            version2.Increment(replicaId);
        } else {
            var version3 = new VersionVector();
            version3.Increment(replicaId);
            _removed[value] = version3;
        }
    }

    public AddWinsORSet<T> Merge(AddWinsORSet<T> other) {
        // Merge add and remove sets, if both sets contain the same key, merge the version vectors

        var added = _added.Concat(other._added).GroupBy(x => x.Key).ToDictionary(group => group.Key, group => {
            // Merge all the version vectors together into a single version vector
            VersionVector v = new();
            foreach (var item in group) {
                v.MergeInPlace(item.Value);
            }

            return v;
        });

        var removed = _removed.Concat(other._removed).GroupBy(x => x.Key).ToDictionary(group => group.Key, group => {
            // Merge all the version vectors together into a single version vector
            VersionVector v = new();
            foreach (var item in group) {
                v.MergeInPlace(item.Value);
            }

            return v;
        });

        // Next, we can trim down both the added and removed dictionaries

        // Remove all values from add map with timestamps LESS THAN values in remove map.
        // This handles concurrent, because we want to favor additions

        var addedFiltered = _added.Where(kv =>
            // Remove all values, where add happened before remove
            removed.TryGetValue(kv.Key, out var ro) && kv.Value.Compare(ro) == VersionVectorComparisonResult.LessThan);

        var removeFiltered = _removed.Where(kv =>
            // Remove all values, where add happened before remove
            added.TryGetValue(kv.Key, out var ro) &&
            kv.Value.Compare(ro) is VersionVectorComparisonResult.Concurrent or VersionVectorComparisonResult.EqualTo or VersionVectorComparisonResult.LessThan);

        // todo is it every possible to have a item in both added and remove lists with an equal version vector?
        //  - in theory, no, but still handle it
        
        var n = new AddWinsORSet<T>();
        foreach (var a in addedFiltered)
            n._added[a.Key] = a.Value;
        foreach (var a in removeFiltered)
            n._removed[a.Key] = a.Value;
        return n;
    }
}