module Validate

// Exception for reporting errors with the position
exception Error of string * (int * int) // position

let reportError msg pos = raise (Error (msg, pos))

let validateExp exp =
    match exp with
    | _ -> failwith "not implemented"