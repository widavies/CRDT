namespace CRDT.Delta;

public struct Dot {
    public int ReplicaId;
    public int Version;
}

public class DotContext {
    public readonly VersionVector Clock = new();
    public readonly ISet<Dot> DotCloud = new HashSet<Dot>();

    // Returns true if the dot context is aware of the event
    // specified by "dot"
    public bool Contains(Dot dot) {
        int? version = Clock.GetVersionForReplica(dot.ReplicaId);
        if(version == null) {
            return DotCloud.Contains(dot);
        } else {
            return version >= dot.Version;
        }
    }

    // replicaId should ONLY be the local replicaId
    // basically, this dot CANNOT be detached
    public Dot NextDot(int replicaId) {
        return new Dot {
            ReplicaId = replicaId,
            Version = Clock.Increment(replicaId)
        };
    }

    public void Add(Dot dot) {
        DotCloud.Add(dot);
    }

    public void MergeInPlace(DotContext context) {
        Clock.MergeInPlace(context.Clock);
        foreach(var dot in context.DotCloud) {
            DotCloud.Add(dot);
        }

        // Compact
        CompactInPlace();
    }

    public void CompactInPlace() {
        ISet<Dot> toRemove = new HashSet<Dot>();

        // Import to traverse in consecutive order, otherwise we might not remove some dots
        // that we should remove
        foreach(Dot dot in DotCloud.OrderBy(x => x.ReplicaId).ThenBy(x => x.Version)) {
            int clockVersion = Clock.GetVersionForReplica(dot.ReplicaId) ?? 0;

            if(dot.Version == clockVersion + 1) {
                Clock.Increment(dot.ReplicaId);
                toRemove.Add(dot);
            } else if(dot.Version <= clockVersion) {
                toRemove.Add(dot);
            } else {
                // Do nothing
            }
        }

        foreach(Dot dot in toRemove) {
            DotCloud.Remove(dot);
        }
    }
}

public class DotKernel<T> where T : IEquatable<T> {
    public readonly DotContext DotContext = new();

    // Only contains information about present elements,
    // notice how we don't need to store removed elements!
    public Dictionary<Dot, T> Entries = new();

    public ISet<T> Values => new HashSet<T>(Entries.Values);

    public DotKernel<T>? Delta;

    public void MergeInPlace(DotKernel<T> other) {
        // Add all unseen elements in other to self
        Dictionary<Dot, T> active = new(Entries);

        foreach(var kv in other.Entries) {
            if(!(Values.Contains(kv.Value) || DotContext.Contains(kv.Key))) {
                active[kv.Key] = kv.Value;
            }
        }

        // Check if any elements in self need to be removed (other specifies a remove
        // occurred if the dot for an element is in the dot context, but not in the elements)

        Dictionary<Dot, T> final = new(active);

        foreach(var kv in Entries) {
            if(other.DotContext.Contains(kv.Key) && !other.Values.Contains(kv.Value)) {
                final.Remove(kv.Key);
            }
        }

        Entries = final;
        DotContext.MergeInPlace(other.DotContext);
    }

    public void Add(int replicaId, T value) {
        Dot dot = DotContext.NextDot(replicaId);
        Entries[dot] = value;

        if(Delta == null) {
            Delta = new();
        }

        Delta?.Entries.Add(dot, value);
        Delta?.DotContext.Add(dot);
        Delta?.DotContext.CompactInPlace();
    }

    public void Remove(int replicaId, T value) {
        KeyValuePair<Dot, T>? kv = Entries.FirstOrDefault(x => x.Value.Equals(value));

        if(kv.HasValue) {
            Entries.Remove(kv.Value.Key);
            DotContext.Add(kv.Value.Key);
            
            DotContext.CompactInPlace();
        }
    }
}