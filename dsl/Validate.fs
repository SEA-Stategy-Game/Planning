module Validate

open FParsec

let pCommands, pCommandsRef = createParserForwardedToRef<Lang.Command, unit>()


let keywords: string list = ["if"; "while"]

let unitCommands : string list = ["hp"; "pos"]
let actions: string list = ["MoveTo"; "Collect"; "Harvest"]

let resources = ["Gold"; "Rock"; "Tree"]

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

let pENDIF : Parser<unit, unit> =
    spaces >>. str "END if" .>> ws >>% ()

let pEND : Parser<unit, unit> =
    ws >>. str "END" .>> ws .>> opt newline >>% ()


let pCondition : Parser<string, unit> =
    many1Chars (noneOf "\n") .>> ws


let pActionCommand : Parser<Lang.Command, unit> =
    pipe2
        (choice (actions |> List.map str) .>> ws)
        (many1 (choice (resources |> List.map str) .>> ws))
        (fun action resources ->
            Lang.Command.Action(action, resources))
let pIfCommand : Parser<Lang.Command, unit> =
    pipe2
        (str "if" >>. ws >>. pCondition)
        (manyTill pCommands pENDIF)
        (fun cond innerCommands ->
            Lang.Command.If(cond, innerCommands)
        )


let pIdent =
    many1Chars (letter <|> digit)

let pDotPath =
    sepBy1 pIdent (pchar '.')

let pUnitCommand : Parser<Lang.Command, unit> =
    pipe3
        (str "Unit")
        (many1Chars digit)
        (pchar '.' >>. pDotPath)
        (fun _ unitId path ->
            Lang.Command.UnitCommand(unitId, path)
        )

do pCommandsRef :=
    ws >>. (attempt pIfCommand <|> attempt pUnitCommand <|> pActionCommand) .>> ws


let pUserId : Parser<string, unit> =
    str "unit" .>> ws >>. alphanumeric .>> str ":" .>> ws


let rec commandsToSteps (startIndex:int) (commands: Lang.Command list) : Lang.Step list * int =
    let folder (steps, idx) cmd =
        match cmd with
        | Lang.Command.Action(action, parameters) ->
            let step : Lang.Step =
                {
                    stepIndex = idx
                    stepType = action
                    actionType = action
                    parameters = parameters
                    body = []
                }
            (steps @ [step], idx + 1)

        | Lang.Command.If(cond, inner) ->
            let innerSteps, _ = commandsToSteps 0 inner

            let step : Lang.Step =
                {
                    stepIndex = idx
                    stepType = "if"
                    actionType = "Conditional"
                    parameters = [cond]
                    body = innerSteps
                }

            (steps @ [step], idx + 1)

        | Lang.Command.UnitCommand(id, unitCmd) ->
            let step : Lang.Step =
                {
                    stepIndex = idx
                    stepType = "Unit command"
                    actionType = "Unit" + id
                    parameters = unitCmd
                    body = []
                }

            (steps @ [step], idx + 1)

    List.fold folder ([], startIndex) commands

let pUnitPlan : Parser<Lang.UnitPlanHeader, unit> =
    pipe2 pUserId (manyTill pCommands pEND) (fun uid commands ->
        let steps, _  = commandsToSteps 0 commands
        {
            unitId = uid
            steps = steps
        }) 


let parsePlan : Parser<Lang.Plan, unit> =
    pipe2 pPlanHeader (many pUnitPlan) (fun header units ->
        {
            planHeader = header
            unitPlans = units
        })