module Lang

    type Expr =
    | Int of int
    | Paren of Expr
    | InfixApp of (Expr * Operator * Expr)
    and Operator =
    | MulOp
    | DivOp
    | AddOp
    | SubOp

    let ppExpr e =
        match e with
        | Int i -> string i
        | _ -> "?"

    let ppPlan p =
        match p with
        | InfixApp (e1, op, e2) ->
            match op with
            | AddOp -> printfn "Plus %s, %s" (ppExpr e1) (ppExpr e2)
            | _ -> printfn "no"
        | _ -> printfn "whoops"