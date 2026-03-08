module Program
open FSharp.Text.Lexing
open Compile

[<EntryPoint>]
let main argv =
    let source = "10 + 2" // plain text fra UI
    let parse plan =
        let lexbuf = LexBuffer<char>.FromString plan
        let res = Parser.start Lexer.read lexbuf
        match res with
        | Some ast -> ast
        | None -> failwith "failed to parse"

    let ast = parse source
    //let compiled = Compile.compilePlan ast
    // let pp = PrettyPrint.ppPlan
    0