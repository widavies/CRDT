// See https://aka.ms/new-console-template for more information

using CRDT;

var c1 = new GCounter();
c1.Increment(1);
c1.Increment(1);
c1.Increment(1);

var c2 = new GCounter();
c2.Increment(2);

c1.Merge(c2);

c2.Increment(2);
c2.Increment(2);

c1.Increment(1);

c1.Merge(c2);
c1.Merge(c2);
c1.Merge(c2);
c1.Merge(c2);
c1.Merge(c2);
c2.Merge(c1);

Console.WriteLine($"The counter has a value of {c1.Value} = {c2.Value}");

var pn1 = new PNCounter();
var pn2 = new PNCounter();

pn1.Increment(1);
pn1.Increment(1);

pn2.Decrement(2);

pn1.Merge(pn2);
pn2.Merge(pn1);

Console.WriteLine($"The pn counter has a value of {pn1.Value} = {pn2.Value}");