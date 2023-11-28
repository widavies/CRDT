namespace CRDT; 

// Positive/negative counter
public class PNCounter {
    private readonly GCounter _increments = new();
    private readonly GCounter _decrements = new();

    public int Value => _increments.Value - _decrements.Value;

    public void Increment(int replicaId) {
        _increments.Increment(replicaId);
    }

    public void Decrement(int replicaId) {
        _decrements.Increment(replicaId);
    }

    public void Merge(PNCounter counter) {
        _increments.MergeInPlace(counter._increments);
        _decrements.MergeInPlace(counter._decrements);
    }
}

// Todo: Bounded Counters