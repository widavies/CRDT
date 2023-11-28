namespace CRDT.Delta; 

public class D_PNCounter {
    private readonly D_GCounter _increments = new();
    private readonly D_GCounter _decrements = new();

    public D_PNCounter() {}

    private D_PNCounter(D_GCounter incs, D_GCounter decs) {
        _increments = incs;
        _decrements = decs;
    }
    
    public int Value => _increments.Value - _decrements.Value;

    public void Increment(int replicaId) {
        _increments.Increment(replicaId);
    }

    public void Decrement(int replicaId) {
        _decrements.Increment(replicaId);
    }

    public void MergeInPlace(D_PNCounter counter) {
        _increments.MergeInPlace(counter._increments);
        _decrements.MergeInPlace(counter._decrements);
    }
    
    // Split (full, delta)
    // This just makes it easy to send over the delta to merge with another guy
    public (D_PNCounter, D_PNCounter?) Split() {
        if (_increments._delta == null && _decrements._delta == null) {
            return (this, null);
        } else {
            // Deltas
            var dIncs = _increments._delta ?? new D_GCounter();
            var dDecs = _increments._delta ?? new D_GCounter();

            var pn = new D_PNCounter();
            pn.MergeInPlace(this);
            pn._increments._delta = null;
            pn._decrements._delta = null;
            
            return (pn, new D_PNCounter(dIncs, dDecs));
        }
    }
}