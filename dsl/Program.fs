module Program
open Lang
open FSharp.Text.Lexing
open Compile
open System
open System.IO
open System.Text.Json
open Validate

type planHeader = {
    schemaVersion: int
    gameId: string
    playerId: string
}

type unitPlan = {
    stepIndex: int
    stepType: string // <- stepTypes i stedet
    actionType: string // <- actionTypes i stedet
    parameters: string list// liste af resourcer og andet typer
}

type unitPlanHeader = {
    unitId: string
    steps: unitPlan list
}

let mutable recordList : unitPlanHeader list = []

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
    //let validated_ast = Validate.validateExp ast
    let plan1 = {
        unitId = "12"
        steps = [{
            stepIndex = 1
            stepType = "Action"
            actionType = "MoveTo"
            parameters = ["Base"]
        }]
    }
    let plan2 = {
        unitId = "13"
        steps = [{
            stepIndex = 1
            stepType = "Collect"
            actionType = "Harvest"
            parameters = ["goldOre"]
        };
        {
            stepIndex = 2
            stepType = "Collect"
            actionType = "Harvest"
            parameters = ["ironOre"]
        }]
    }
    recordList <- recordList @ [plan1]
    recordList <- recordList @ [plan2]

    let options = JsonSerializerOptions()
    options.WriteIndented <- true
    let json = JsonSerializer.Serialize(recordList, options)

    File.WriteAllText("person.json", json)
    Lang.ppPlan ast
    0
    //let compiled = Compile.compilePlan ast
    // let pp = PrettyPrint.ppPlan