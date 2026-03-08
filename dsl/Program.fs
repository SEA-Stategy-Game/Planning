module Program
open Lang
open FSharp.Text.Lexing
open Compile

[<EntryPoint>]
let main argv =
    let source = "10 + 2" // source bliver plain text fra Graphics
    let parse plan =
        let lexbuf = LexBuffer<char>.FromString plan
        let res = Parser.start Lexer.read lexbuf
        match res with
        | Some ast -> ast
        | None -> failwith "failed to parse"

    let ast = parse source
    Lang.ppPlan ast
    0
    //let compiled = Compile.compilePlan ast
    // let pp = PrettyPrint.ppPlan