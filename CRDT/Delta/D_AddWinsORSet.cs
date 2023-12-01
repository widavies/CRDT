namespace CRDT.Delta; 

// Fixes two problems:
// - We have to always store tombstones
// - Metadata gets quite large (we store a full vector clock for every member of the set)
//
// Create a dot context - a vector clock of the most recent contiguous update, plus a set of all dots floating
// - No dot cloud for ourself, we're always "up-to-date" with ourself
public class D_AddWinsORSet<T> where T : IEquatable<T> {
    public DotKernel<T> Kernel = new();
    
    public ISet<T> Value => Kernel.Values;
    
    public void Add(int replicaId, T value) {
        Kernel.Remove(replicaId, value); 
        Kernel.Add(replicaId, value);
    }

    public void Remove(int replicaId, T value) {
        Kernel.Remove(replicaId, value);
    }

    public void MergeInPlace(D_AddWinsORSet<T> other) {
        Kernel.MergeInPlace(other.Kernel);
    }
    
}