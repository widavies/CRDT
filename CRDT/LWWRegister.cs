namespace CRDT;

// Provides "LWW" behavior
public class LWWRegister<T> {
    public T? Value;
    public DateTime LastWrite { get; set; } = DateTime.MinValue;

    public void Set(T? t, DateTime time) {
        if (time > LastWrite) {
            Value = t;
            LastWrite = time;
        }
    }

    public void Set(T? t) {
        Set(t, DateTime.Today);
    }

    public void MergeInPlace(LWWRegister<T> other) {
        if (LastWrite < other.LastWrite) {
            Set(other.Value);
        }
    }
    
    // todo: Usually, the timestamp is only considered when there is a "concurrent" update (to resolve the concurrency difference)
    // use a lamport clock? (basically the same as HLC)
    //
    // Theory: Every register uses a VersionVector (of HLCs).
    // Trivial to update if non-conflicting, otherwise, compare the physical time portion of the timestamp to determine which came later
    //https://adamwulf.me/2021/05/distributed-clocks-and-crdts/#:~:text=What%E2%80%99s%20particularly%20nice%20about%20this%20clock%2C%20too%2C%20is%20how%20it%20handles%20a%20misbehaving%20wall%20clock.%20Jared%20does%20a%20great%20job%20explaining%20why%3A
}

public class MVRegister<T> {
    // Detects concurrent writes and saves both values
}

// todo make sure HLC uses UTC time

// literally, a LWW should just use a bunch of HLC, when a concurrent update is made, gather all of the "concurrently" problematic
// values and take the most recent one.