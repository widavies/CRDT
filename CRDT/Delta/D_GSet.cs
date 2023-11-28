namespace CRDT.Delta; 

// Observation:
// - Two delta updates for set, if one is lost, we don't send everything
// - Counters, each future delta contains all previous deltas
public class D_GSet<T> {

    public readonly HashSet<T> Values = new();

    public D_GSet<T>? Delta;

    public void Add(T value) {
        Values.Add(value);

        if (Delta == null) {
            Delta = new D_GSet<T>();
        }
        
        Delta?.Add(value);
    }

    public void MergeInPlace(D_GSet<T> other) {
        foreach (var val in other.Values) {
            Values.Add(val);
        }

        if (Delta != null && other.Delta != null) {
            Delta.MergeInPlace(other.Delta);
        } else if (Delta == null && other.Delta != null) {
            Delta = new D_GSet<T>();
            Delta.MergeInPlace(other.Delta);
        }
    }
    
    // Split into full & delta
    public (D_GSet<T>, D_GSet<T>?) Split() {
        var n = new D_GSet<T>();
        n.MergeInPlace(this);
        n.Delta = null;

        return (n, Delta);
    }
}
