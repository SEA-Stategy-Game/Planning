module Validate

open FParsec

let actions: string list = ["MoveTo"; "Collect"; "Harvest"]

let parameters = ["Gold"; "Rock"; "Tree"]

let str s = pstring s
let ws = spaces

let alphanumeric = many1Chars (letter <|> digit)

let pVersion : Parser<int32, unit> =
    str "Schema version:" >>. spaces1 >>. pint32 .>> ws

let pGameId : Parser<string, unit> = 
    str "Game Id:" >>. spaces1 >>. alphanumeric .>> ws

let pPlayerId : Parser<string, unit> =
    str "Player Id:" >>. spaces1 >>. alphanumeric .>> ws

let pPlanHeader : Parser<Lang.PlanHeader, unit> =
    pipe3 pVersion pGameId pPlayerId (fun version gid pid -> 
        {
            schemaVersion = version
            gameId = gid
            playerId = pid
        })


let pEND : Parser<unit, unit>=
    str "END" .>> ws >>% ()

let pCommands : Parser<string * string list, unit> =
    pipe2 
        (choice (actions |> List.map str) .>> ws)
        (many1 (choice (parameters |> List.map str) .>> ws))
        (fun action parameters -> action, parameters) 

let pUserId : Parser<string, unit> =
    str "unit" .>> ws >>. alphanumeric .>> str ":" .>> ws

let mkStep index (action, parameters) : Lang.Step =
    {
        stepIndex = index
        stepType = "Action"
        actionType = action
        parameters = parameters
    }

let pUnitPlan : Parser<Lang.UnitPlanHeader, unit> =
    pipe2 pUserId (manyTill pCommands pEND) (fun uid commands ->
        {
            unitId = uid
            steps = commands |> List.mapi mkStep
        }) 

let parsePlan : Parser<Lang.Plan, unit> =
    pipe2 pPlanHeader (many pUnitPlan) (fun header units->
        {
            planHeader = header
            unitPlans = units
        })