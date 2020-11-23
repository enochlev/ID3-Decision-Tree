CLI arguments
<input Training File> <outputTraining File / Optional, by default replaces .in with .out>

ex.

"C://..../Decision Tree Project.exe" "C://..../golf.in

outputs gold.out in the same directory


file.in structure

_________________________
<number of attributes>
<attribute Name> <possible attribute values/"continuous" if a continuous value>
...
...
<attribute Name> <possible attribute values/"continuous" if a continuous value>
<attribute #1 training value> <attribute #2 training value> .... <attribute #n training value> <binary output>
______________________________________________________________________________________


The output sorts the decision tree by information gain of each attribute