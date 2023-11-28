# CRDTs
Conflict-free replicated data types

Some popular implementations:
- Amazon Dynamo
- Riak
- Cassandra/Scylla

## State based CRDTs
Merging state based CRDTs must conform to the following properties:
- Commutative
- Associativity
- Idempotency

Some basic operations that meet these criteria:
- Union of sets
- Maximum of two values

### G-Counter
Short for "grow-only counter". Every replica has one operation "increment". The value of the counter will eventually converge
to the sum of increments across all replicas.

Use cases:
- Page view