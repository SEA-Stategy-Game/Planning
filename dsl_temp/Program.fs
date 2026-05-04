module Program
open System
open System.IO
open System.Text.Json
open FParsec
open Validate

[<EntryPoint>]
let main argv =
    if argv.Length < 2 then
        eprintfn "Usage: dsl <inputFile> <outputFile>"
        1
    else
        let inputPath = argv.[0]
        let outputPath = argv.[1]
        let input = File.ReadAllText(inputPath).Trim()

        let options = JsonSerializerOptions()
        options.PropertyNamingPolicy <- JsonNamingPolicy.SnakeCaseLower
        options.WriteIndented <- true

        match run parsePlan input with
        | Success(value, _, _) ->
            let json = JsonSerializer.Serialize(value, options)
            File.WriteAllText(outputPath, json)
            printfn "OK"
            0
        | Failure(msg, _, _) ->
            eprintfn "Parse error: %s" msg
            2
