module Program
open Lang
open System
open System.IO
open System.Text.Json
open FParsec
open Validate


[<EntryPoint>]
let main argv =
    let source = "10 + 2" // source bliver plain text fra Graphics
    //let validated_ast = Validate.validateExp ast
    let step = "Test"
    let plan1 = {
        unitId = "12"
        steps = [{
            stepIndex = 1
            stepType = step
            actionType = "MoveTo"
            parameters = ["Base"]
        }]
    }
    let plan2 = {
        unitId = "13"
        steps = [{
            stepIndex = 1
            stepType = step
            actionType = "Harvest"
            parameters = ["goldOre"]
        };
        {
            stepIndex = 2
            stepType = step
            actionType = "Harvest"
            parameters = ["ironOre"]
        }]
    }

    let options = JsonSerializerOptions()
    options.WriteIndented <- true

    let input = stdin.ReadToEnd().Trim()
    match run parsePlan input with
        | Success(value, _, _) ->  
            let json = JsonSerializer.Serialize(value, options)
            File.WriteAllText("person.json", json)
            printfn "nice"
        | Failure(msg, _, _) -> printfn "Fuck: %s" msg 
    0
    //let compiled = Compile.compilePlan ast
    // let pp = PrettyPrint.ppPlan