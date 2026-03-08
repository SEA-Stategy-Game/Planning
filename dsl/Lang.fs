module Lang

module AST =

    type Expr =
    | Int of int
    | Paren of Expr
    | InfixApp of (Expr * Operator * Expr)
    and Operator =
    | MulOp
    | DivOp
    | AddOp
    | SubOp

module PrettyPrint =
    let ppPlan p =
        failwith ("not implemented")