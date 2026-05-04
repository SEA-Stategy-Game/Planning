module Program
open Lang
open System
open System.IO
open System.Text
open System.Text.Json
open FParsec
open Validate
open System.Net.Http

[<EntryPoint>]
let main argv =
    let options = JsonSerializerOptions()
    options.WriteIndented <- true

    let input = File.ReadAllText(argv[0])

    match run parsePlan input with
        | Success(value, _, _) ->  
            let json = JsonSerializer.Serialize(value, options)
            async {
                try
                    use client = new HttpClient()
                    let content =
                        new StringContent(
                            json,
                            Encoding.UTF8,
                            "application/json"
                        )
                    let! request = client.PostAsync("http://localhost:5050/", content)  // SKRIV DET RIGTIGE ENDPOINT :)
                                |> Async.AwaitTask
                    let! body = request.Content.ReadAsStringAsync() |> Async.AwaitTask
                    printfn "Status: %A" request.StatusCode
                    printfn "Response body: %s" body
                    printfn "Succesfully sent the validated plan to the planning backend"
                with ex ->
                    let message = "Request failed with msg: " + ex.ToString()
                    in failwith message
            }
            |> Async.RunSynchronously
            File.WriteAllText("plan.json", json)

        | Failure(msg, _, _) -> printfn "Something went wrong?: %s" msg 
    0