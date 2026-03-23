UnitPlans   ::= UnitPlan
            | epsilon

UnitPlan    ::= "unit" int ":" uStepList END
            ::= "building" int ":" bStepList END

uStepList    ::= 
            | uStep uStepList
            | epsilon

bStepList   ::=
            | bStep bStepList
            | epsilon

uStep       ::= 
            | "moveTo" MoveParams
            | "collect" Resources

bStep       ::= 
            | "spawn" Unit

Unit        ::=
            | 

MoveParams  ::=
            | Buildings
            | Resources

Buildings   ::=
            | "goldmine"

Resources   ::=
            | "tree"
            | "rock"
            | "gold"

END         ::= UnitPlans