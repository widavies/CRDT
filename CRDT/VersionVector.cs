using System.Net.Http.Headers;

namespace CRDT;

// Problems using timestamps in distributed systems
// - Clock skews, bugs, invalid time values, etc.
// - Timestamps don't tell us anything about casualty (whether one update is aware of another)

public enum VersionVectorComparisonResult {
    // The incoming version vector is "less than" the local vector. This means the
    // local vector represents ALL events that the incoming vector does (and several more), so the incoming vector
    // can be discarded with no further consideration. However, this may trigger a push to the other device.
    LessThan = -1,

    // The incoming vector summarizes exacts the same set of events our local vector does, and vice versa.
    // The vector can be discarded and nothing needs to be synced.
    EqualTo = 0,

    // The incoming vector has more changes than we do. We should incorporate them into our state and then
    // update our vector
    GreaterThan = 1,

    // Events occurred concurrently (to us, they may have been serial, but we can't tell and will treat them as concurrent),
    // so we have some events the incoming replica doesn't have, and they have events we don't have. We will need
    // to get two new version vectors by merging theirs into ours and ours into theirs.
    Concurrent = 2
}

// This is a simple example where every replica will increment a value locally.
// The one downside of this (and the reason to use hybrid logical clocks in the future is to get some sense
// of "when" something was updated)
// Todo: Will HLCs give an actual timestamp precise down to the millisecond?
public class VersionVector {
    private readonly GCounter _counter = new();

    public void MergeInPlace(VersionVector other) {
        _counter.Merge(other._counter);
    }

    public int Increment(int replicaId) {
        _counter.Increment(replicaId);
        return _counter.Values[replicaId];
    }

    public int? GetVersionForReplica(int replicaId) {
        if(_counter.Values.TryGetValue(replicaId, out var version)) {
            return version;
        } else {
            return null;
        }
    }
    
    public VersionVectorComparisonResult Compare(VersionVector other) {
        // Todo faster improvement to this?
        Dictionary<int, int> local = new(_counter.Values);

        foreach (var kv in other._counter.Values.Keys.Where(x => !local.ContainsKey(x))) {
            local.Add(kv, 0);
        }

        Dictionary<int, int> incoming = new(other._counter.Values);

        foreach (var kv in _counter.Values.Keys.Where(x => !incoming.ContainsKey(x))) {
            incoming.Add(kv, 0);
        }

        if (local.All(kv => incoming[kv.Key] == kv.Value)) {
            return VersionVectorComparisonResult.EqualTo;
        } else if (local.All(kv => incoming[kv.Key] >= kv.Value)) {
            return VersionVectorComparisonResult.GreaterThan;
        } else if (local.All(kv => incoming[kv.Key] <= kv.Value)) {
            return VersionVectorComparisonResult.LessThan;
        } else {
            return VersionVectorComparisonResult.Concurrent;
        }
    }
    
    // Todo a better version of this will use a hybrid logical clock with a version vector, i.e.,
    // version vector whose value is a HLC value
}