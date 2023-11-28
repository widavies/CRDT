namespace CRDT.Delta; 

// Fixes two problems:
// - We have to always store tombstones
// - Metadata gets quite large
//
// Create a dot context - a vector clock of the most recent contiguous update, plus a set of all dots floating
// - No dot cloud for ourself, we're always "up-to-date" with ourself
public class D_AddWinsORSet {
    
}