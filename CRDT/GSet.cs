namespace CRDT; 

// A grow only set
// Literally just a normal set, except that we exclude ourselves
// from using the remove operation
// If we were to remove, it would just show up again - bad!
public class GSet<T> {

    public readonly HashSet<T> Values = new();

    public void Add(T value) {
        Values.Add(value);
    }

    public void MergeInPlace(GSet<T> other) {
        foreach (var val in other.Values) {
            Values.Add(val);
        }
    }
}