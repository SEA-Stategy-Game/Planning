UnitPlans   ::= UnitPlan
            | epsilon

UnitPlan    ::= "unit" int ":" StepList END

StepList    ::= 
            | Step StepList
            | epsilon

Step        ::= 
            | "moveTo" MoveParams
            | "collect" Resources

MoveParams  ::=
            | Buildings
            | Resources

Buildings   ::=
            | "base"
            | "goldmine"

Resources   ::=
            | "tree"
            | "rock"
            | "gold"

END         ::= UnitPlans